using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FishyFlip.Lexicon;
using FishyFlip.Lexicon.App.Bsky.Actor;
using FishyFlip.Models;
using FishyFlip.Tools;
using Humanizer;
using UniSky.Extensions;
using UniSky.Services;
using UniSky.ViewModels.Feeds;
using UniSky.ViewModels.Profiles;
using Windows.Foundation.Metadata;
using Windows.Phone;
using Windows.UI.ViewManagement;

namespace UniSky.ViewModels.Profile;

public partial class ProfilePageViewModel : ProfileViewModel
{
    [ObservableProperty]
    private string bannerUrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Followers))]
    private int followerCount;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Following))]
    private int followingCount;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Posts))]
    private int postCount;

    public string Followers
        => FollowerCount.ToMetric(decimals: 2);
    public string Following
        => FollowingCount.ToMetric(decimals: 2);
    public string Posts
        => PostCount.ToMetric(decimals: 2);

    public ObservableCollection<FeedViewModel> Feeds { get; }

    public FeedViewModel SelectedFeed
        => Feeds[0];

    public ProfilePageViewModel() : base() { }

    public ProfilePageViewModel(ATObject profile, IProtocolService protocolService)
        : base(profile)
    {
        if (profile is ProfileViewDetailed detailed)
        {
            Populate(detailed);
        }
        else
        {
            _ = Task.Run(LoadAsync);
        }

        Feeds =
        [
            new ProfileFeedViewModel("posts_no_replies", profile, protocolService),
            new ProfileFeedViewModel("posts_with_replies", profile, protocolService),
            new ProfileFeedViewModel("posts_with_media", profile, protocolService)
        ];

        // TODO: calculate the brightness of the banner image
    }

    private async Task LoadAsync()
    {
        using var context = this.GetLoadingContext();

        var protocol = Ioc.Default.GetRequiredService<IProtocolService>()
            .Protocol;

        var profile = (await protocol.GetProfileAsync(this.id).ConfigureAwait(false))
            .HandleResult();

        Populate(profile);
    }

    private void Populate(ProfileViewDetailed profile)
    {
        BannerUrl = profile.Banner;
        FollowerCount = (int)profile.FollowersCount;
        FollowingCount = (int)profile.FollowsCount;
        PostCount = (int)profile.PostsCount;
    }

    protected override void OnLoadingChanged(bool value)
    {
        if (!ApiInformation.IsApiContractPresent(typeof(PhoneContract).FullName, 1))
            return;

        this.syncContext.Post(() =>
        {
            var statusBar = StatusBar.GetForCurrentView();
            _ = statusBar.ShowAsync();

            statusBar.ProgressIndicator.ProgressValue = null;

            if (value)
            {
                _ = statusBar.ProgressIndicator.ShowAsync();
            }
            else
            {
                _ = statusBar.ProgressIndicator.HideAsync();
            }
        });
    }

}
