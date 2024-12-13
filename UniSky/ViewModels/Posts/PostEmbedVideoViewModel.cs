using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip.Lexicon.App.Bsky.Embed;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;

namespace UniSky.ViewModels.Posts;

public partial class PostEmbedVideoViewModel : PostEmbedViewModel
{
    private readonly ViewVideo video;

    [ObservableProperty]
    private string thumbnailUrl;
    [ObservableProperty]
    private IMediaPlaybackSource source;
    [ObservableProperty]
    private AspectRatioConstraint ratio;

    public PostEmbedVideoViewModel(ViewVideo video)
        : base(video)
    {
        this.video = video;
        this.ThumbnailUrl = video.Thumbnail;
        this.Ratio = video.AspectRatio != null ?
            new AspectRatioConstraint(Math.Max((double)video.AspectRatio.Width.Value / video.AspectRatio.Height.Value, 0.5)) :
            new AspectRatioConstraint(16, 9);

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
