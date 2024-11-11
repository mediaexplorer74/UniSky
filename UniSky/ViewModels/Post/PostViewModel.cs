using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using FishyFlip.Models;
using FishyFlip.Tools;
using Humanizer;
using UniSky.Pages;
using UniSky.Services;
using UniSky.ViewModels.Feeds;
using UniSky.ViewModels.Profiles;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace UniSky.ViewModels.Posts;

public partial class PostViewModel : ViewModelBase
{
    private readonly PostView view;

    private ATUri like;
    private ATUri repost;

    [ObservableProperty]
    private string text;
    [ObservableProperty]
    private ProfileViewModel author;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Likes))]
    private int likeCount;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Retweets))]
    private int retweetCount;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Replies))]
    private int replyCount;

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

    public string Likes
        => ToNumberString(LikeCount);
    public string Retweets
        => ToNumberString(RetweetCount);
    public string Replies
        => ToNumberString(ReplyCount);

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
        Embed = CreateEmbedViewModel(view.Embed);

        LikeCount = view.LikeCount;
        RetweetCount = view.RepostCount;
        ReplyCount = view.ReplyCount;

        if (view.Viewer is not null)
        {
            IsRetweeted = view.Viewer.Repost != null;

            if (view.Viewer.Like is not null)
            {
                this.like = view.Viewer.Like;
                IsLiked = true;
            }

            if (view.Viewer.Repost is not null)
            {
                this.repost = view.Viewer.Repost;
                IsRetweeted = true;
            }
        }
    }

    [RelayCommand]
    private async Task Like()
    {
        var protocol = Ioc.Default.GetRequiredService<IProtocolService>()
            .Protocol;

        if (IsLiked)
        {
            var like = Interlocked.Exchange(ref this.like, null); // not stressed about threading here, just cleaner way to exchange values
            if (like == null)
                return;

            IsLiked = false;
            LikeCount--;

            _ = (await protocol.Repo.DeleteLikeAsync(like.Rkey).ConfigureAwait(false))
                .HandleResult();
        }
        else
        {
            IsLiked = true;
            LikeCount++;

            this.like = (await protocol.Repo.CreateLikeAsync(view.Cid, view.Uri).ConfigureAwait(false))
                .HandleResult()?.Uri;
        }
    }

    [RelayCommand]
    private void OpenProfile(UIElement element)
    {
        var service = Ioc.Default.GetRequiredService<INavigationServiceLocator>()
            .GetNavigationService("Home");

        service.Navigate<ProfilePage>(this.view.Author, new ContinuumNavigationTransitionInfo() { ExitElement = element });
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
            VideoViewEmbed video => new PostEmbedVideoViewModel(video),
            _ => null,
        };
    }
}
