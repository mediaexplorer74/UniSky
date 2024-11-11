using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FishyFlip.Models;
using UniSky.Services;
using UniSky.ViewModels.Feeds;

namespace UniSky.ViewModels.Profile;

public partial class ProfileFeedViewModel : FeedViewModel
{
    public ProfileFeedViewModel(AuthorFeedFilterType filterType, FeedProfile profile, IProtocolService protocolService)
        : base(FeedType.Author)
    {
        this.Name = filterType switch
        {
            AuthorFeedFilterType.PostsNoReplies => "Posts",
            AuthorFeedFilterType.PostsWithReplies => "Replies",
            AuthorFeedFilterType.PostsWithMedia => "Media",
            AuthorFeedFilterType.PostsAndAuthorThreads => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };

        this.Items = new FeedItemCollection(this, FeedType.Author, profile.Did, filterType, protocolService);
    }
}
