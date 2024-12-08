using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishyFlip.Tools;
using Microsoft.Extensions.Logging;
using UniSky.Controls.Compose;
using UniSky.Extensions;
using UniSky.Helpers;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace UniSky.ViewModels.Compose;

public enum ComposeViewAttachmentType
{
    Image, Video
}

public partial class ComposeViewAttachmentViewModel : ViewModelBase
{
    private readonly ComposeViewModel parent;
    private readonly ILogger<ComposeViewAttachmentViewModel> logger;

    [ObservableProperty]
    private BitmapImage thumbnail;
    [ObservableProperty]
    private string altText;

    public ComposeViewAttachmentType AttachmentType { get; private set; }
    public bool IsTemporary { get; private set; }
    public IStorageFile StorageFile { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public string ContentType { get; private set; }


    public ComposeViewAttachmentViewModel(ComposeViewModel parent,
                                          IStorageFile storageFile,
                                          ComposeViewAttachmentType type,
                                          bool isTemporary,
                                          ILogger<ComposeViewAttachmentViewModel> logger)
    {
        this.parent = parent;
        this.logger = logger;

        this.IsTemporary = isTemporary;
        this.StorageFile = storageFile;
        this.AttachmentType = type;
        this.ContentType = storageFile.ContentType;
        this.AltText = "";

        Task.Run(LoadAsync);
    }

    public new void SetErrored(Exception ex)
    {
        base.SetErrored(ex);
    }

    private async Task LoadAsync()
    {
        using var loading = this.GetLoadingContext();

        try
        {
            if (this.AttachmentType == ComposeViewAttachmentType.Image)
            {
                await CompressImageAsync(StorageFile, CheckHeifSupport());
            }

            if (this.StorageFile is not IStorageItemProperties properties)
                return;

            var thumbStream = await properties.GetThumbnailAsync(ThumbnailMode.SingleItem, 512);
            syncContext.Post(async () =>
            {
                try
                {
                    Thumbnail ??= new BitmapImage();
                    await Thumbnail.SetSourceAsync(thumbStream);
                    thumbStream.Dispose();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to set thumbnail!");
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set thumbnail!");
        }
    }

    [RelayCommand]
    private void Remove()
    {
        this.parent.AttachedFiles.Remove(this);
    }

    [RelayCommand]
    private async Task AddAltTextAsync()
    {
        var previousAltText = AltText;
        var altTextDialog = new ComposeAddAltTextDialog(this);
        if (parent.SheetController != null && ApiInformation.IsApiContractPresent(typeof(UniversalApiContract).FullName, 8))
            altTextDialog.XamlRoot = parent.SheetController.Root.XamlRoot;

        if (await altTextDialog.ShowAsync() != ContentDialogResult.Primary)
            AltText = previousAltText;
    }

    protected override void OnLoadingChanged(bool value)
    {
        base.OnLoadingChanged(value);

        syncContext.Post(() => parent.UpdateLoading(this, value));
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

    private async Task CompressImageAsync(IStorageFile input, bool useHeif, int size = 2048)
    {
        var extension = useHeif ? "heic" : "jpeg";
        var contentType = useHeif ? "image/heic" : "image/jpeg";
        var codec = useHeif ? BitmapEncoder.HeifEncoderId : BitmapEncoder.JpegEncoderId;

        var output = await ApplicationData.Current.TemporaryFolder.CreateFileAsync($"{Guid.NewGuid()}.{extension}");

        try
        {
            double width, height;
            using (var inputStream = await input.OpenAsync(FileAccessMode.Read))
            using (var outputStream = await output.OpenAsync(FileAccessMode.ReadWrite))
            {
                var decoder = await BitmapDecoder.CreateAsync(inputStream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                width = (int)decoder.OrientedPixelWidth;
                height = (int)decoder.OrientedPixelHeight;

                do
                {
                    outputStream.Size = 0;

                    SizeHelpers.Scale(ref width, ref height, size, size);

                    var encoder = await BitmapEncoder.CreateAsync(codec, outputStream);
                    encoder.SetSoftwareBitmap(softwareBitmap);
                    encoder.BitmapTransform.ScaledWidth = (uint)Math.Ceiling(width);
                    encoder.BitmapTransform.ScaledHeight = (uint)Math.Ceiling(height);
                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;

                    await encoder.FlushAsync();

                    contentType = encoder.EncoderInformation.MimeTypes.FirstOrDefault() ?? contentType;
                    size = (int)Math.Floor(size * 0.75);
                }
                while (outputStream.Size > 1_000_000);
            }

            if (IsTemporary)
            {
                await TryDeleteTemporaryFile(input);
            }

            Width = (int)Math.Ceiling(width);
            Height = (int)Math.Ceiling(height);
            ContentType = contentType;
            IsTemporary = true;
            StorageFile = output;
        }
        catch (Exception ex) when ((uint)ex.HResult == 0xc00d5212) // missing heif codec
        {
            await output.DeleteAsync(StorageDeleteOption.PermanentDelete);
            await CompressImageAsync(input, false, size);
        }
        catch (Exception ex)
        {
            SetErrored(ex);
        }
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
