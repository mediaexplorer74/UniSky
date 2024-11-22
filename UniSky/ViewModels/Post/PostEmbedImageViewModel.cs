using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip.Lexicon.App.Bsky.Embed;
using FishyFlip.Models;

namespace UniSky.ViewModels.Posts;

public partial class PostEmbedImageViewModel : ViewModelBase
{
    [ObservableProperty]
    private string thumbnailUrl;

    public PostEmbedImageViewModel(ViewImage image)
    {
        ThumbnailUrl = image.Thumb;
    }
}
