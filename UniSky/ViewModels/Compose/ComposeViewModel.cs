using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using FishyFlip.Lexicon;
using FishyFlip.Lexicon.App.Bsky.Actor;
using FishyFlip.Lexicon.App.Bsky.Embed;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Lexicon.Com.Atproto.Repo;
using FishyFlip.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniSky.Extensions;
using UniSky.Helpers;
using UniSky.Helpers.Interop;
using UniSky.Services;
using UniSky.ViewModels.Posts;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace UniSky.ViewModels.Compose;

public partial class ComposeViewModel : ViewModelBase
{
    private static readonly string[] IMAGE_FILE_EXTENSIONS =
        [".jpg", ".jpeg", ".jfif", ".png", ".gif", ".webp", ".avif", ".tiff"];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPost))]
    [NotifyPropertyChangedFor(nameof(Characters))]
    private string _text;
    [ObservableProperty]
    private string _avatarUrl;
    [ObservableProperty]
    private int maxCharacters;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasReply))]
    private PostViewModel replyTo;

    private readonly ResourceLoader resources;
    private readonly IProtocolService protocolService;
    private readonly ILogger<ComposeViewModel> logger;

    // TODO: this but better
    public bool IsDirty
        => (!string.IsNullOrEmpty(Text) || HasAttachments);
    // TODO: ditto
    public bool CanPost
        => (!string.IsNullOrEmpty(Text) || HasAttachments) && Text.Length <= 300;
    public int Characters
        => Text?.Length ?? 0;

    public bool HasReply
        => ReplyTo != null;

    public ObservableCollection<ComposeViewAttachmentViewModel> AttachedFiles { get; }
    public bool HasAttachments
        => AttachedFiles.Count > 0;

    public ComposeViewModel(IProtocolService protocolService,
                            ILogger<ComposeViewModel> logger,
                            PostViewModel replyTo = null)
    {
        this.protocolService = protocolService;
        this.logger = logger;
        this.resources = ResourceLoader.GetForCurrentView();

        this.ReplyTo = replyTo;
        this.MaxCharacters = 300;
        this.AttachedFiles = [];
        this.AttachedFiles.CollectionChanged += (o, ev) =>
        {
            this.SetErrored(null);
            this.OnPropertyChanged(nameof(HasAttachments));
            this.OnPropertyChanged(nameof(CanPost));
        };

        Task.Run(LoadAsync);
    }

    private async Task LoadAsync()
    {
        using var loading = this.GetLoadingContext();

        var protocol = protocolService.Protocol;

        try
        {
            var profile = (await protocol.GetProfileAsync(protocol.AuthSession.Session.Did)
                .ConfigureAwait(false))
                .HandleResult();

            AvatarUrl = profile.Avatar;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch user info!");
            this.SetErrored(ex);
        }
    }

    [RelayCommand]
    private async Task PostAsync()
    {
        this.SetErrored(null);
        using var ctx = this.GetLoadingContext();

        try
        {
            var text = Text;
            var replyRef = await GetReplyDefAsync().ConfigureAwait(false);

            ATObject embed = await GetImageEmbed()
                .ConfigureAwait(false);

            var postModel = new Post(text, reply: replyRef, embed: embed);
            var post = (await protocolService.Protocol.CreatePostAsync(postModel)
                .ConfigureAwait(false))
                .HandleResult();

            Text = null;
            syncContext.Post(async () => { await Hide(); });
        }
        catch (Exception ex)
        {
            this.SetErrored(ex);
        }
    }

    private async Task<ATObject> GetImageEmbed()
    {
        EmbedImages embed = null;
        foreach (var image in this.AttachedFiles.Where(f => f.AttachmentType == ComposeViewAttachmentType.Image))
        {
            var properties = await image.StorageFile.GetBasicPropertiesAsync()
                .AsTask().ConfigureAwait(false);

            if (properties.Size > 1_000_000)
                throw new InvalidOperationException("Attached image is too large!"); // useless, bad, awful

            using var stream = await image.StorageFile.OpenStreamForReadAsync()
                .ConfigureAwait(false);
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);

            var blob = (await protocolService.Protocol.UploadBlobAsync(content)
                .ConfigureAwait(false))
                .HandleResult();

            embed ??= new EmbedImages([]);
            embed.Images.Add(new Image(blob.Blob, image.AltText, new AspectRatio(image.Width, image.Height)));
        }

        return embed;
    }

    [RelayCommand]
    private async Task Hide()
    {
        var sheetService = Ioc.Default.GetRequiredService<ISheetService>();
        await sheetService.TryCloseAsync();
    }

    [RelayCommand]
    private async Task AddPhotoAsync()
    {
        try
        {
            this.SetErrored(null);

            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                CommitButtonText = $"Upload to Bluesky",
                ViewMode = PickerViewMode.Thumbnail
            };

            foreach (var type in IMAGE_FILE_EXTENSIONS)
                picker.FileTypeFilter.Add(type);

            var files = await picker.PickMultipleFilesAsync();
            foreach (var file in files)
            {
                await AddFileAsync(file, false);
            }
        }
        catch (Exception ex)
        {
            this.SetErrored(ex);
        }
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            this.SetErrored(null);

            var picker = new CameraCaptureUI();
            picker.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            picker.PhotoSettings.AllowCropping = false;

            var file = await picker.CaptureFileAsync(CameraCaptureUIMode.Photo);
            await AddFileAsync(file, false);
        }
        catch (Exception ex)
        {
            this.SetErrored(ex);
        }
    }

    private async Task<ReplyRefDef> GetReplyDefAsync()
    {
        ReplyRefDef replyRef = null;
        if (ReplyTo == null)
            return replyRef;

        var replyPost = ReplyTo.View;
        var replyRecord = (await protocolService.Protocol.GetRecordAsync(replyPost.Uri.Did, replyPost.Uri.Collection, replyPost.Uri.Rkey, replyPost.Cid)
            .ConfigureAwait(false))
            .HandleResult();

        if (replyRecord.Value is not Post replyPostFetched)
            throw new InvalidOperationException(resources.GetString("E_ReplyToNonPost"));

        var replyPostReplyDef = replyPostFetched.Reply;

        replyRef = new ReplyRefDef()
        {
            Root = replyPostReplyDef?.Root ?? new StrongRef() { Uri = replyPost.Uri, Cid = replyPost.Cid },
            Parent = new StrongRef() { Uri = replyPost.Uri, Cid = replyPost.Cid }
        };

        return replyRef;
    }

    internal bool HandlePaste()
    {
        var dataPackageView = Clipboard.GetContent();
        if (dataPackageView.Contains(StandardDataFormats.StorageItems) ||
            dataPackageView.Contains(StandardDataFormats.Bitmap) ||
            dataPackageView.Contains("DeviceIndependentBitmapV5"))
        {
            _ = DoPasteAsync();
            return true;
        }

        return false;
    }

    private async Task DoPasteAsync()
    {
        try
        {
            this.SetErrored(null);

            var dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains("DeviceIndependentBitmapV5"))
            {
                var data = (IRandomAccessStream)await dataPackageView.GetDataAsync("DeviceIndependentBitmapV5");
                var file = await BitmapInterop.SaveBitmapToFileAsync(data);
                await AddFileAsync(file, true);

                return;
            }

            if (dataPackageView.Contains(StandardDataFormats.Bitmap))
            {
                var data = await dataPackageView.GetBitmapAsync();
                var file = await BitmapInterop.SaveBitmapToFileAsync(data);
                await AddFileAsync(file, true);

                return;
            }

            if (dataPackageView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await dataPackageView.GetStorageItemsAsync();
                foreach (var file in items.OfType<IStorageFile>())
                    await AddFileAsync(file, true);

                return;
            }
        }
        catch (Exception ex)
        {
            this.SetErrored(ex);
        }
    }

    private async Task AddFileAsync(IStorageFile storageFile, bool isTemporary)
    {
        // TODO: may not always cover webp/avif. dig into this.
        var type = storageFile.ContentType.StartsWith("image/") ? ComposeViewAttachmentType.Image :
                   storageFile.ContentType.StartsWith("video") ? ComposeViewAttachmentType.Video :
                   throw new InvalidOperationException(resources.GetString("E_NonImageOrVideo"));

        var width = 0;
        var height = 0;
        var contentType = storageFile.ContentType;

        if (type == ComposeViewAttachmentType.Image)
        {
            if (AttachedFiles.Any(t => t.AttachmentType == ComposeViewAttachmentType.Video))
                throw new InvalidOperationException(resources.GetString("E_UnableToAddImageToVideoPost"));

            if (AttachedFiles.Where(t => t.AttachmentType == ComposeViewAttachmentType.Image).Count() + 1 > 4)
                throw new InvalidOperationException(resources.GetString("E_TooManyPhotos"));

            var newFile = await CompressImageAsync(storageFile);
            if (isTemporary)
            {
                await TryDeleteTemporaryFile(storageFile);
            }

            storageFile = newFile.file;
            width = newFile.width;
            height = newFile.height;
            contentType = newFile.contentType;
            isTemporary = true;
        }

        if (type == ComposeViewAttachmentType.Video)
        {
            if (AttachedFiles.Any(t => t.AttachmentType == ComposeViewAttachmentType.Image))
                throw new InvalidOperationException(resources.GetString("E_UnableToAddVideoToImagePost"));

            if (AttachedFiles.Where(t => t.AttachmentType == ComposeViewAttachmentType.Video).Count() + 1 > 1)
                throw new InvalidOperationException(resources.GetString("E_TooManyVideos"));

            throw new InvalidOperationException("Videos are currently unsupported.");
        }

        AttachedFiles.Add(ActivatorUtilities.CreateInstance<ComposeViewAttachmentViewModel>(Ioc.Default, this, storageFile, type, isTemporary, width, height, contentType));
    }

    private async Task TryDeleteTemporaryFile(IStorageFile storageFile)
    {
        try
        {
            await storageFile.DeleteAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete temporary file.");
        }
    }

    private async Task<(IStorageFile file, int width, int height, string contentType)> CompressImageAsync(IStorageFile input)
    {
        var useHeif = CheckHeifSupport();
        var output = await ApplicationData.Current.TemporaryFolder.CreateFileAsync($"{Guid.NewGuid()}.{(useHeif ? "heif" : "jpeg")}");
        var contentType = useHeif ? "image/heif" : "image/jpeg";

        double width, height;
        using (var inputStream = await input.OpenAsync(FileAccessMode.Read))
        using (var outputStream = await output.OpenAsync(FileAccessMode.ReadWrite))
        {
            var decoder = await BitmapDecoder.CreateAsync(inputStream);
            width = (int)decoder.PixelWidth;
            height = (int)decoder.PixelHeight;

            SizeHelpers.Scale(ref width, ref height, 2048, 2048);

            var encoder = await BitmapEncoder.CreateAsync(useHeif ? BitmapEncoder.HeifEncoderId : BitmapEncoder.JpegEncoderId, outputStream);
            encoder.SetSoftwareBitmap(await decoder.GetSoftwareBitmapAsync());
            encoder.BitmapTransform.ScaledWidth = (uint)Math.Ceiling(width);
            encoder.BitmapTransform.ScaledHeight = (uint)Math.Ceiling(height);

            await encoder.FlushAsync();
        }

        return (output, (int)Math.Ceiling(width), (int)Math.Ceiling(height), contentType);
    }

    private static bool CheckHeifSupport()
    {
        if (!ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7, 0))
            return false;

        foreach (var item in BitmapEncoder.GetEncoderInformationEnumerator())
        {
            if (item.CodecId == BitmapEncoder.HeifEncoderId)
                return true;
        }

        return false;
    }
}
