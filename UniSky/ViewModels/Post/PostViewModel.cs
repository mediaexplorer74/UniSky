using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using FishyFlip.Models;
using FishyFlip.Tools;
using Humanizer;
using UniSky.Services;
using UniSky.ViewModels.Feeds;
using UniSky.ViewModels.Profiles;

namespace UniSky.ViewModels.Posts;

public partial class PostViewModel : ViewModelBase
{
    private readonly PostView view;

    [ObservableProperty]
    private string text;
    [ObservableProperty]
    private ProfileViewModel author;
    [ObservableProperty]
    private string likes;
    [ObservableProperty]
    private string retweets;
    [ObservableProperty]
    private string replies;

    [ObservableProperty]
    private bool isLiked;
    [ObservableProperty]
    private bool isRetweeted;

    [ObservableProperty]
    private ProfileViewModel retweetedBy;
    [ObservableProperty]
    private ProfileViewModel replyTo;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEmbed))]
    private PostEmbedViewModel embed;

    public bool HasEmbed
        => Embed != null;

    public PostViewModel(FeedViewPost feedPost)
        : this(feedPost.Post)
    {
        if (feedPost.Reason is ReasonRepost { By: not null } repost)
        {
            RetweetedBy = new ProfileViewModel(repost.By);
        }

        if (feedPost.Reply is FeedViewPostReply { Parent: PostView { Author: not null } })
        {
            ReplyTo = new ProfileViewModel(feedPost.Reply.Parent.Author);
        }
    }

    public PostViewModel(PostView view)
    {
        this.view = view;
        Author = new ProfileViewModel(view.Author);
        Text = view.Record.Text;
        Likes = ToNumberString(view.LikeCount);
        Retweets = ToNumberString(view.RepostCount);
        Replies = ToNumberString(view.ReplyCount);
        Embed = CreateEmbedViewModel(view.Embed);

        IsLiked = view.Viewer?.Like != null;
        IsRetweeted = view.Viewer?.Repost != null;
    }

    [RelayCommand]
    private async Task Like()
    {
        var protocol = Ioc.Default.GetRequiredService<IProtocolService>()
            .Protocol;

        var likeRecord = (await protocol.Repo.CreateLikeAsync(view.Cid, view.Uri).ConfigureAwait(false))
            .HandleResult();

        IsLiked = likeRecord != null;
    }

    private string ToNumberString(int n)
    {
        if (n == 0)
            return "\xA0";

        return n.ToMetric(decimals: 2);
    }

    private PostEmbedViewModel CreateEmbedViewModel(Embed embed)
    {
        if (embed is null)
            return null;

        return embed switch
        {
            ImageViewEmbed images => new PostEmbedImagesViewModel(images),
            _ => null,
        };
    }
}
