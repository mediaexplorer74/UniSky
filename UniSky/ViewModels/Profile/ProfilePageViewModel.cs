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
using Windows.UI.Xaml;

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowBio))]
    private string bio;

    [ObservableProperty]
    private ProfileFeedViewModel selectedFeed;

    public Visibility ShowBio
        => !string.IsNullOrWhiteSpace(Bio) ? Visibility.Visible : Visibility.Collapsed;

    public string Followers
        => FollowerCount.ToMetric(decimals: 2);
    public string Following
        => FollowingCount.ToMetric(decimals: 2);
    public string Posts
        => PostCount.ToMetric(decimals: 2);

    public ObservableCollection<ProfileFeedViewModel> Feeds { get; }

    public ProfilePageViewModel() : base() { }

    public ProfilePageViewModel(ATDid did)
    {
        this.id = did;

        Feeds = [];
        SelectedFeed = null;
        Task.Run(LoadAsync);
    }

    public ProfilePageViewModel(ATObject profile)
        : base(profile)
    {
        var protocol = Ioc.Default.GetRequiredService<IProtocolService>();
        if (profile is ProfileViewDetailed detailed)
        {
            Populate(detailed);
        }

        Task.Run(LoadAsync);

        Feeds =
        [
            new ProfileFeedViewModel(this, "posts_no_replies", profile, protocol),
            new ProfileFeedViewModel(this, "posts_with_replies", profile, protocol),
            new ProfileFeedViewModel(this, "posts_with_media", profile, protocol)
        ];

        SelectedFeed = Feeds[0];

        // TODO: calculate the brightness of the banner image
    }

    private async Task LoadAsync()
    {
        using var context = this.GetLoadingContext();

        var protocol = Ioc.Default.GetRequiredService<IProtocolService>();
        var profile = (await protocol.Protocol.GetProfileAsync(this.id).ConfigureAwait(false))
            .HandleResult();

        syncContext.Post(() =>
        {
            if (Feeds.Count == 0)
            {
                Feeds.Add(new ProfileFeedViewModel(this, "posts_no_replies", profile, protocol));
                Feeds.Add(new ProfileFeedViewModel(this, "posts_with_replies", profile, protocol));
                Feeds.Add(new ProfileFeedViewModel(this, "posts_with_media", profile, protocol));

                SelectedFeed = Feeds[0];
            }

            Populate(profile);
        });
    }

    private void Populate(ProfileViewDetailed profile)
    {
        this.id = profile.Did;
        this.AvatarUrl = profile.Avatar;
        this.Name = profile.DisplayName;
        this.Handle = $"@{profile.Handle}";
        this.BannerUrl = profile.Banner;
        this.FollowerCount = (int)profile.FollowersCount;
        this.FollowingCount = (int)profile.FollowsCount;
        this.PostCount = (int)profile.PostsCount;
        this.Bio = profile.Description?.Trim();
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

    internal void Select(ProfileFeedViewModel profileFeedViewModel)
    {
        SelectedFeed = profileFeedViewModel;
    }
}
