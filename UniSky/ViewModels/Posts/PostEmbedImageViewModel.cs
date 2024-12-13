using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip.Lexicon.App.Bsky.Embed;
using FishyFlip.Models;

namespace UniSky.ViewModels.Posts;

public partial class PostEmbedImageViewModel : ViewModelBase
{
    [ObservableProperty]
    private string thumbnailUrl;

    public PostEmbedImageViewModel(ATIdentifier id, Image image)
    {
        ThumbnailUrl = $"https://cdn.bsky.app/img/feed_thumbnail/plain/{id}/{image.ImageValue.Ref.Link}@jpeg";
    }

    public PostEmbedImageViewModel(ViewImage image)
    {
        ThumbnailUrl = image.Thumb;
    }
}
