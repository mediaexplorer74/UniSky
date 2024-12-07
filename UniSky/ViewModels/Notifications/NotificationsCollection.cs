using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;
using System.Web;
using UniSky.Services;
using UniSky.ViewModels.Feeds;
using UniSky.ViewModels.Posts;
using UniSky.ViewModels.Profile;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using FishyFlip.Lexicon.App.Bsky.Notification;
using FishyFlip.Tools;
using System.Collections;
using Windows.UI.Notifications;
using FishyFlip.Lexicon.App.Bsky.Actor;

namespace UniSky.ViewModels.Notifications;

public class NotificationsCollection : ObservableCollection<NotificationViewModel>, ISupportIncrementalLoading
{
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    private readonly CoreDispatcher dispatcher = Window.Current.Dispatcher;

    private readonly NotificationsPageViewModel parent;
    private readonly IProtocolService protocolService;

    private string cursor;

    public NotificationsCollection(NotificationsPageViewModel parent, IProtocolService protocolService)
    {
        this.parent = parent;
        this.protocolService = protocolService;
    }
    public bool HasMoreItems { get; private set; } = true;


    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return Task.Run(async () =>
        {
            try
            {
                await semaphore.WaitAsync();
                return await InternalLoadMoreItemsAsync((int)count);
            }
            finally
            {
                semaphore.Release();
            }
        }).AsAsyncOperation();
    }

    private async Task<LoadMoreItemsResult> InternalLoadMoreItemsAsync(int count)
    {
        var service = protocolService.Protocol;
        var viewModel = parent;
        viewModel.Error = null;

        count = Math.Clamp(count, 5, 25);

        using var context = viewModel.GetLoadingContext();

        try
        {
            var notifications = (await service.ListNotificationsAsync(count, cursor: cursor)
                .ConfigureAwait(false))
                .HandleResult();

            this.cursor = notifications.Cursor;

            var hydratePostIds = notifications.Notifications.Where(n =>
                n.Reason is (NotificationReason.Like or NotificationReason.Repost) &&
                n.ReasonSubject is not null)
                .Select(s => s.ReasonSubject)
                .Distinct();

            var posts = (await service.GetPostsAsync(hydratePostIds.ToList())
                .ConfigureAwait(false))
                .HandleResult();

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var groups = notifications.Notifications
                    .GroupBy(g => (g.Reason is (NotificationReason.Like or NotificationReason.Repost)) ? string.Join('-', g.Reason, g.ReasonSubject) : null);
                foreach (var group in groups)
                {
                    NotificationViewModel viewModel;
                    if (group.Key != null && (viewModel = this.FirstOrDefault(v => v.Subject != null && v.Key == group.Key)) != null)
                    {
                        viewModel.Add(group);
                        continue;
                    }

                    if (group.Key == null)
                    {
                        foreach (var notification in group)
                        {
                            PostView post = null;
                            if (notification.Reason is (NotificationReason.Like or NotificationReason.Repost))
                                post = posts.Posts.FirstOrDefault(p => p.Uri.ToString() == notification.ReasonSubject.ToString());

                            Add(new NotificationViewModel(notification, post));
                        }
                    }
                    else
                    {
                        var notification = group.FirstOrDefault();

                        PostView post = null;
                        if (notification.Reason is (NotificationReason.Like or NotificationReason.Repost))
                            post = posts.Posts.FirstOrDefault(p => p.Uri.ToString() == notification.ReasonSubject.ToString());

                        Add(new NotificationViewModel(group, post));
                    }
                }

                ArrayList.Adapter(this).Sort(); // ?????
            });

            if (notifications.Notifications.Count == 0 || string.IsNullOrWhiteSpace(this.cursor))
                HasMoreItems = false;

            return new LoadMoreItemsResult() { Count = (uint)notifications.Notifications.Count };
        }
        catch (Exception ex)
        {
            HasMoreItems = false;
            return new LoadMoreItemsResult() { Count = 0 };
        }
    }
}
