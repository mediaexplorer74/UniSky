using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishyFlip.Tools;
using Microsoft.Extensions.Logging;
using UniSky.Controls.Compose;
using UniSky.Extensions;
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
                                          int width,
                                          int height,
                                          string contentType,
                                          ILogger<ComposeViewAttachmentViewModel> logger)
    {
        this.parent = parent;
        this.logger = logger;

        this.IsTemporary = isTemporary;
        this.StorageFile = storageFile;
        this.AttachmentType = type;
        this.Width = width;
        this.Height = height;
        this.ContentType = contentType;
        this.AltText = "";

        Task.Run(LoadAsync);
    }

    private async Task LoadAsync()
    {
        using var loading = this.GetLoadingContext();

        try
        {
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
        if (await altTextDialog.ShowAsync() != ContentDialogResult.Primary)
            AltText = previousAltText;
    }
}
