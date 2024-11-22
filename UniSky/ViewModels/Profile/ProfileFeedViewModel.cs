using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FishyFlip.Lexicon;
using FishyFlip.Lexicon.App.Bsky.Actor;
using FishyFlip.Models;
using UniSky.Services;
using UniSky.ViewModels.Feeds;

namespace UniSky.ViewModels.Profile;

public partial class ProfileFeedViewModel : FeedViewModel
{
    public ProfileFeedViewModel(string filterType, ATObject profile, IProtocolService protocolService)
        : base(FeedType.Author, protocolService)
    {
        this.Name = filterType switch
        {
            "posts_no_replies" => "Posts",
            "posts_with_replies" => "Replies",
            "posts_with_media" => "Media",
            "posts_and_author_threads" => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };

        var did = profile switch
        {
            ProfileViewBasic basic => basic.Did,
            ProfileViewDetailed detailed => detailed.Did,
            ProfileView view => view.Did,
            _ => throw new InvalidCastException()
        };
        this.Items = new FeedItemCollection(this, FeedType.Author, did, filterType, protocolService);
    }
}
