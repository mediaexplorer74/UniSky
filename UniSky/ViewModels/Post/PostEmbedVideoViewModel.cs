using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip.Models;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using WinAspectRatio = Microsoft.Toolkit.Uwp.UI.Controls.AspectRatio;

namespace UniSky.ViewModels.Posts;

public partial class PostEmbedVideoViewModel : PostEmbedViewModel
{
    private readonly VideoViewEmbed video;

    [ObservableProperty]
    private string thumbnailUrl;
    [ObservableProperty]
    private IMediaPlaybackSource source;
    [ObservableProperty]
    private WinAspectRatio ratio;


    public PostEmbedVideoViewModel(VideoViewEmbed video)
        : base(video)
    {
        this.video = video;
        this.ThumbnailUrl = video.Thumbnail;
        this.Ratio = video.AspectRatio != null ?
            new WinAspectRatio(video.AspectRatio.Width, video.AspectRatio.Height) :
            new WinAspectRatio(16, 9);

        // todo: lazy
        _ = Task.Run(LoadAsync);
    }

    private async Task LoadAsync()
    {
        var create = await AdaptiveMediaSource.CreateFromUriAsync(new Uri(video.Playlist));
        if (create.Status == AdaptiveMediaSourceCreationStatus.Success)
            Source = MediaSource.CreateFromAdaptiveMediaSource(create.MediaSource);
    }
}
