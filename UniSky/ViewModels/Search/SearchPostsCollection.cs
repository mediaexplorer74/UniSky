using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Tools;
using UniSky.Services;
using UniSky.ViewModels.Posts;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace UniSky.ViewModels.Search;

public class SearchPostsCollection : ObservableCollection<PostViewModel>, ISupportIncrementalLoading
{
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    private readonly CoreDispatcher dispatcher = Window.Current.Dispatcher;
    private readonly IProtocolService protocolService;
    private readonly HashSet<string> ids = [];
    private readonly SearchFeedViewModel parent;

    private readonly string query;
    private readonly string searchType;

    private string cursor;

    public SearchPostsCollection(string query, string searchType, SearchFeedViewModel parent, IProtocolService protocolService)
    {
        this.parent = parent;
        this.protocolService = protocolService;
        this.query = query;
        this.searchType = searchType;
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

        count = Math.Clamp(count, 5, 100);

        using var context = viewModel.GetLoadingContext();

        try
        {
            var results = (await protocolService.Protocol.SearchPostsAsync(this.query, this.searchType, limit: count, cursor: this.cursor)
                .ConfigureAwait(false))
                .HandleResult();

            this.cursor = results.Cursor;

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var item in results.Posts)
                {
                    if (!ids.Contains(item.Cid))
                        Add(new PostViewModel(item));
                }
            });

            if (results.Posts.Count == 0 || string.IsNullOrWhiteSpace(this.cursor))
                HasMoreItems = false;

            return new LoadMoreItemsResult() { Count = (uint)results.Posts.Count };
        }
        catch (Exception ex)
        {
            HasMoreItems = false;
            return new LoadMoreItemsResult() { Count = 0 };
        }
    }
}
