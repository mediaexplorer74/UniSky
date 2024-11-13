using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip;
using FishyFlip.Events;
using FishyFlip.Models;
using FishyFlip.Tools;
using Microsoft.Extensions.Logging;
using UniSky.Extensions;
using UniSky.Helpers;
using UniSky.Models;
using UniSky.Pages;
using UniSky.Services;
using Windows.Foundation.Metadata;
using Windows.Phone;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UniSky.ViewModels;

// Keep in sync with HomePage.xaml
public enum HomePages
{
    Home,
    Search,
    Notifications,
    Chat,
    Profile
}

public partial class HomeViewModel : ViewModelBase
{
    private readonly ATProtocol protocol;
    private readonly SessionService sessionService;
    private readonly INavigationService rootNavigationService;
    private readonly INavigationService homeNavigationService;
    private readonly ILogger<HomeViewModel> logger;
    private readonly IProtocolService protocolService;
    private readonly SessionModel sessionModel;
    private readonly DispatcherTimer notificationUpdateTimer;

    private FeedProfile profile;

    [ObservableProperty]
    private string _avatarUrl;

    [ObservableProperty]
    private string _displayName;

    [ObservableProperty]
    public int _notificationCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HomeSelected),
        nameof(SearchSelected),
        nameof(NotificationsSelected),
        nameof(ChatSelected),
        nameof(ProfileSelected))]
    private HomePages _page = (HomePages)(-1);

    public bool HomeSelected
        => Page == HomePages.Home;
    public bool SearchSelected
        => Page == HomePages.Search;
    public bool NotificationsSelected
        => Page == HomePages.Notifications;
    public bool ChatSelected
        => Page == HomePages.Chat;
    public bool ProfileSelected
        => Page == HomePages.Profile;

    public HomeViewModel(
        string session,
        SessionService sessionService,
        INavigationServiceLocator navigationServiceLocator,
        IProtocolService protocolService,
        ILogger<HomeViewModel> logger,
        ILogger<ATProtocol> protocolLogger)
    {
        this.rootNavigationService = navigationServiceLocator.GetNavigationService("Root");
        this.homeNavigationService = navigationServiceLocator.GetNavigationService("Home");

        if (!sessionService.TryFindSession(session, out var sessionModel))
        {
            rootNavigationService.Navigate<LoginPage>();
            return;
        }

        ApplicationData.Current.LocalSettings.Values["LastUsedUser"] = session;

        this.sessionService = sessionService;
        this.logger = logger;
        this.protocolService = protocolService;
        this.sessionModel = sessionModel;

        this.protocol = new ATProtocolBuilder()
            .WithLogger(protocolLogger)
            .Build();

        protocolService.SetProtocol(protocol);

        // TODO: throttle when in background
        this.notificationUpdateTimer = new DispatcherTimer() { Interval = TimeSpan.FromMinutes(1) };
        this.notificationUpdateTimer.Tick += OnNotificationTimerTick;

        Task.Run(LoadAsync);
    }

    private async Task LoadAsync()
    {
        using var loading = this.GetLoadingContext();

        try
        {
            var session = await this.protocol.AuthenticateWithPasswordSessionAsync(sessionModel.Session);
            var refreshSession = await this.protocol.RefreshAuthSessionAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to authenticate!");
            this.syncContext.Post(() => rootNavigationService.Navigate<LoginPage>());
            return;
        }

        this.Page = HomePages.Home;

        try
        {
            await Task.WhenAll(UpdateProfileAsync(), UpdateNotificationsAsync())
                .ConfigureAwait(false);

            this.syncContext.Post(() => notificationUpdateTimer.Start());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch user info!");
            this.SetErrored(ex);
        }
    }

    private async Task UpdateProfileAsync()
    {
        profile = (await this.protocol.Actor.GetProfileAsync(protocol.Session.Did)
            .ConfigureAwait(false))
            .HandleResult();

        AvatarUrl = profile.Avatar;
        DisplayName = profile.DisplayName;
    }

    private async Task UpdateNotificationsAsync()
    {
        var notifications = (await this.protocol.Notification.GetUnreadCountAsync()
            .ConfigureAwait(false))
            .HandleResult();

        NotificationCount = notifications.Count;
    }

    private async void OnNotificationTimerTick(object sender, object e)
    {
        try
        {
            await UpdateNotificationsAsync()
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update notifications");
        }
    }

    partial void OnPageChanged(HomePages oldValue, HomePages newValue)
    {
        if (oldValue != newValue)
        {
            this.syncContext.Post(NavigateToPage);
        }
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

    private void NavigateToPage()
    {
        switch (Page)
        {
            case HomePages.Home:
                this.homeNavigationService.Navigate<FeedsPage>();
                break;
            case HomePages.Search:
            case HomePages.Notifications:
            case HomePages.Chat:
                this.homeNavigationService.Navigate<Page>();
                break;
            case HomePages.Profile:
                this.homeNavigationService.Navigate<ProfilePage>(this.profile);
                break;
        }
    }

    internal void UpdateChecked()
    {
        this.OnPropertyChanged(nameof(HomeSelected),
        nameof(SearchSelected),
        nameof(NotificationsSelected),
        nameof(ChatSelected),
        nameof(ProfileSelected));
    }
}
