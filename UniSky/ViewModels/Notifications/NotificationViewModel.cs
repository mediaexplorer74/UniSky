using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip.Lexicon.App.Bsky.Embed;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Lexicon.App.Bsky.Notification;
using FishyFlip.Models;
using Humanizer;
using UniSky.ViewModels.Posts;
using UniSky.ViewModels.Profile;
using Windows.ApplicationModel.Resources;

namespace UniSky.ViewModels.Notifications;

public static class NotificationReason
{
    public const string Like = "like",
                        Repost = "repost",
                        Follow = "follow",
                        Mention = "mention",
                        Reply = "reply",
                        Quote = "quote",
                        StarterpackJoined = "starterpack-joined";
}

public partial class NotificationViewModel : ViewModelBase, IComparable, IComparable<NotificationViewModel>
{
    private class NotificationComparer : IEqualityComparer<Notification>
    {
        public bool Equals(Notification x, Notification y)
        {
            return x?.Cid == y?.Cid;
        }

        public int GetHashCode(Notification obj)
        {
            return obj.Cid.GetHashCode();
        }
    }

    private readonly Post subjectPost;
    private readonly ATIdentifier subjectPostAuthor;

    [ObservableProperty]
    private string notificationTitle;
    [ObservableProperty]
    private string notificationSubtitle;
    [ObservableProperty]
    private DateTime timestamp;
    [ObservableProperty]
    private PostEmbedViewModel notificationEmbed;
    [ObservableProperty]
    private string avatarUrl;

    public ATUri Subject { get; }
    public string Reason { get; }

    public bool ShowAvatar
        => Reason is (NotificationReason.Follow or NotificationReason.Reply or NotificationReason.Quote);
    public bool IsRetweet
        => Reason == NotificationReason.Repost;
    public bool IsLike
        => Reason == NotificationReason.Like;

    public string Key =>
        string.Join('-', Reason, Subject);
    public int Count =>
        Notifications.Count;

    public HashSet<Notification> Notifications { get; }
        = new HashSet<Notification>(new NotificationComparer());

    private Notification MostRecent
        => Notifications.OrderByDescending(d => d.IndexedAt.Value)
                        .FirstOrDefault();

    public NotificationViewModel(Notification notification, PostView post = null)
    {
        this.subjectPost = post?.Record as Post ?? notification.Record as Post;
        this.subjectPostAuthor = post?.Author?.Did ?? notification.Author.Did;
        Subject = notification.ReasonSubject;
        Notifications.Add(notification);
        Timestamp = notification.IndexedAt.Value;
        Reason = notification.Reason;
        Update();
    }

    public NotificationViewModel(IEnumerable<Notification> notifications, PostView post = null)
        : this(notifications.FirstOrDefault(), post)
    {
        foreach (var item in notifications)
            Notifications.Add(item);

        Timestamp = MostRecent.IndexedAt.Value;
        Update();
    }

    internal void Add(IEnumerable<Notification> items)
    {
        foreach (var item in items)
            Notifications.Add(item);

        Timestamp = MostRecent.IndexedAt.Value;
        Update();
    }

    private void Update()
    {
        var resources = ResourceLoader.GetForCurrentView();
        var other = resources.GetString("NotificationOther");
        var mostRecentAuthor = new ProfileViewModel(MostRecent.Author);

        switch (Reason)
        {
            case NotificationReason.Like:
                {
                    if (Count == 1)
                        NotificationTitle = string.Format(resources.GetString("NotificationLikedTweetSingle"), mostRecentAuthor.Name);
                    else
                        NotificationTitle = string.Format(resources.GetString("NotificationLikedTweetMultiple"), mostRecentAuthor.Name, other.ToQuantity(Count - 1));

                    break;
                }
            case NotificationReason.Repost:
                {
                    if (Count == 1)
                        NotificationTitle = string.Format(resources.GetString("NotificationRetweetSingle"), mostRecentAuthor.Name);
                    else
                        NotificationTitle = string.Format(resources.GetString("NotificationRetweetMultiple"), mostRecentAuthor.Name, other.ToQuantity(Count - 1));

                    break;
                }
            case NotificationReason.Follow:
                NotificationTitle = string.Format(resources.GetString("NotificationFollow"), mostRecentAuthor.Name);
                break;
            case NotificationReason.Reply:
                NotificationTitle = string.Format(resources.GetString("NotificationReply"), mostRecentAuthor.Name);
                break;
            case NotificationReason.Mention:
                NotificationTitle = string.Format(resources.GetString("NotificationMention"), mostRecentAuthor.Name);
                break;
            case NotificationReason.Quote:
                NotificationTitle = string.Format(resources.GetString("NotificationQuote"), mostRecentAuthor.Name);
                break;
        }

        AvatarUrl = mostRecentAuthor.AvatarUrl;
        NotificationSubtitle = subjectPost?.Text;
        if (subjectPost is { Embed: EmbedImages and { } images })
        {
            NotificationEmbed = new PostEmbedImagesViewModel(subjectPostAuthor, images);
        }
    }

    public int CompareTo(object obj)
    {
        return -((IComparable)Timestamp).CompareTo(((NotificationViewModel)obj).Timestamp);
    }

    public int CompareTo(NotificationViewModel other)
    {
        return Timestamp.CompareTo(other.Timestamp);
    }
}
