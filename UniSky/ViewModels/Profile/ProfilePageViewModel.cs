using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
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

    public ProfilePageViewModel(FeedProfile profile, IProtocolService protocolService) : base(profile)
    {
        BannerUrl = profile.Banner;
        FollowerCount = profile.FollowersCount;
        FollowingCount = profile.FollowsCount;
        PostCount = profile.PostsCount;

        Feeds =
        [
            new ProfileFeedViewModel(AuthorFeedFilterType.PostsNoReplies, profile, protocolService),
            new ProfileFeedViewModel(AuthorFeedFilterType.PostsWithReplies, profile, protocolService),
            new ProfileFeedViewModel(AuthorFeedFilterType.PostsWithMedia, profile, protocolService)
        ];

        _ = Task.Run(LoadAsync);
    }

    private async Task LoadAsync()
    {
        using var context = this.GetLoadingContext();

        var protocol = Ioc.Default.GetRequiredService<IProtocolService>()
            .Protocol;

        var profile = (await protocol.Actor.GetProfileAsync(this.id).ConfigureAwait(false))
            .HandleResult();

        BannerUrl = profile.Banner;
        FollowerCount = profile.FollowersCount;
        FollowingCount = profile.FollowsCount;
        PostCount = profile.PostsCount;
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
