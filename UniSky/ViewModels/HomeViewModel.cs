using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishyFlip;
using FishyFlip.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniSky.Controls.Settings;
using UniSky.Extensions;
using UniSky.Models;
using UniSky.Pages;
using UniSky.Services;
using Windows.Foundation.Metadata;
using Windows.Phone;
using Windows.Storage;
using Windows.UI.Core;
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
    Feeds,
    Lists,
    Chat,
    Profile,
    Settings
}

public partial class HomeViewModel : ViewModelBase
{
    private readonly ISessionService sessionService;
    private readonly INavigationService rootNavigationService;
    private readonly INavigationService homeNavigationService;
    private readonly ILogger<HomeViewModel> logger;
    private readonly ILogger<ATProtocol> atLogger;
    private readonly IProtocolService protocolService;
    private readonly INotificationsService notificationsService;
    private readonly IModerationService moderationService;
    private readonly SessionModel sessionModel;

    private readonly DispatcherTimer refreshTokenTimer;

    private bool isLoaded;

    [ObservableProperty]
    private MenuItemViewModel _selectedMenuItem;

    public ObservableCollection<MenuItemViewModel> MenuItems { get; } = [];
    public ObservableCollection<MenuItemViewModel> FooterMenuItems { get; } = [];
    public ObservableCollection<MenuItemViewModel> PinnedMenuItems { get; } = [];

    public HomeViewModel(
        string profile,
        ISessionService sessionService,
        INavigationServiceLocator navigationServiceLocator,
        IProtocolService protocolService,
        INotificationsService notificationsService,
        IModerationService moderationService,
        ILogger<HomeViewModel> logger,
        ILogger<ATProtocol> protocolLogger)
    {
        this.rootNavigationService = navigationServiceLocator.GetNavigationService("Root");
        this.homeNavigationService = navigationServiceLocator.GetNavigationService("Home");

        if (!sessionService.TryFindSession(profile, out var sessionModel))
        {
            rootNavigationService.Navigate<LoginPage>();
            return;
        }

        ApplicationData.Current.LocalSettings.Values["LastUsedUser"] = profile;

        this.sessionService = sessionService;
        this.logger = logger;
        this.protocolService = protocolService;
        this.sessionModel = sessionModel;
        this.notificationsService = notificationsService;
        this.moderationService = moderationService;
        this.atLogger = protocolLogger;

        var protocol = new ATProtocolBuilder()
            .WithLogger(atLogger)
            .EnableAutoRenewSession(true)
            .WithSessionRefreshInterval(TimeSpan.FromMinutes(30))
            .WithUserAgent(Constants.UserAgent)
            .Build();

        protocolService.SetProtocol(protocol);

        MenuItems.Add(new MenuItemViewModel(this, HomePages.Home, "\uE80F", typeof(FeedsPage)));
        MenuItems.Add(new MenuItemViewModel(this, HomePages.Search, "\uE71E", typeof(SearchPage)));
        MenuItems.Add(new NotificationsMenuItemViewModel(this));
        MenuItems.Add(new MenuItemViewModel(this, HomePages.Feeds, "\uE728", typeof(Page)));
        MenuItems.Add(new MenuItemViewModel(this, HomePages.Lists, "\uE71D", typeof(Page)));
        MenuItems.Add(new MenuItemViewModel(this, HomePages.Chat, "\uE8F2", typeof(Page)));

        FooterMenuItems.Add(new ProfileMenuItemViewModel(this));
        FooterMenuItems.Add(new MenuItemViewModel(this, HomePages.Settings, "\uE713", typeof(Page)));

        PinnedMenuItems.Add(MenuItems[0]);
        PinnedMenuItems.Add(MenuItems[1]);
        PinnedMenuItems.Add(MenuItems[2]);
        PinnedMenuItems.Add(MenuItems[5]);
        PinnedMenuItems.Add(FooterMenuItems[0]);

        // TODO: throttle when in background

        this.refreshTokenTimer = new DispatcherTimer() { Interval = TimeSpan.FromHours(1) };
        this.refreshTokenTimer.Tick += OnRefreshTokenTimerTick;

        var navigationManager = SystemNavigationManager.GetForCurrentView();
        navigationManager.BackRequested += OnBackRequested;

        var window = Window.Current;
        window.Activated += OnWindowActivatedAsync;
        window.VisibilityChanged += OnVisibilityChanged;

        //Task.Run(LoadAsync);
    }

    [RelayCommand]
    private void GoBack()
    {
        this.homeNavigationService.GoBack();
    }

    private async Task LoadAsync()
    {
        if (isLoaded)
            return;

        isLoaded = true;

        using var loading = this.GetLoadingContext();
        var protocol = this.protocolService.Protocol;

        try
        {
            await RefreshSessionAsync()
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to authenticate!");
            this.syncContext.Post(() => rootNavigationService.Navigate<LoginPage>());
            return;
        }

        await moderationService.ConfigureModerationAsync()
            .ConfigureAwait(false);

        SelectedMenuItem = MenuItems[0];

        try
        {
            await this.notificationsService.InitializeAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start notifications service!");
        }

        await Task.WhenAll(
            MenuItems.Concat(FooterMenuItems)
                     .Select(s => s.LoadAsync()));
    }

    private Task RefreshSessionAsync()
    {
        return protocolService.RefreshSessionAsync(sessionModel);
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        var sheetService = ServiceContainer.Scoped.GetRequiredService<ISheetService>();
        await sheetService.ShowAsync<SettingsSheet>();
    }

    private async void OnRefreshTokenTimerTick(object sender, object e)
    {
        try
        {
            await RefreshSessionAsync()
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to authenticate!");
            this.syncContext.Post(() => rootNavigationService.Navigate<LoginPage>());
            return;
        }
    }

    private void OnBackRequested(object sender, BackRequestedEventArgs e)
    {
        if (homeNavigationService.CanGoBack)
        {
            e.Handled = true;
            homeNavigationService.GoBack();
        }
    }

    private void OnVisibilityChanged(object sender, VisibilityChangedEventArgs e)
    {
        if (e.Visible)
        {
            Task.Run(LoadAsync);
        }
    }

    private async void OnWindowActivatedAsync(object sender, WindowActivatedEventArgs e)
    {
        if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            return;

        try
        {
            await RefreshSessionAsync()
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to authenticate!");
            return;
        }
    }


    partial void OnSelectedMenuItemChanging(MenuItemViewModel oldValue, MenuItemViewModel newValue)
    {
        if (oldValue == newValue) return;

        if (oldValue != null)
            oldValue.IsSelected = false;

        if (newValue != null)
            newValue.IsSelected = true;

        this.syncContext.Post(() =>
        {
            var serviceLocator = ServiceContainer.Scoped.GetRequiredService<INavigationServiceLocator>();
            var service = serviceLocator.GetNavigationService("Home");
            service.Frame = newValue.Content;
        });
    }

    protected override void OnLoadingChanged(bool value)
    {
        if (!ApiInformation.IsApiContractPresent(typeof(PhoneContract).FullName, 1))
            return;

        this.syncContext.Post(() =>
        {
            try
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
            }
            catch { }
        });
    }

    //private void NavigateToPage()
    //{
    //    switch (Page)
    //    {
    //        case HomePages.Home:
    //            this.homeNavigationService.Navigate<FeedsPage>();
    //            break;
    //        case HomePages.Search:
    //            this.homeNavigationService.Navigate<SearchPage>();
    //            break;
    //        case HomePages.Notifications:
    //            this.homeNavigationService.Navigate<NotificationsPage>();
    //            break;
    //        case HomePages.Feeds:
    //        case HomePages.Lists:
    //        case HomePages.Chat:
    //            this.homeNavigationService.Navigate<Page>();
    //            break;
    //        case HomePages.Profile:
    //            this.homeNavigationService.Navigate<ProfilePage>(this.profile);
    //            break;
    //    }
    //}

    //internal void UpdateChecked()
    //{
    //    this.OnPropertyChanged(nameof(HomeSelected),
    //            nameof(SearchSelected),
    //            nameof(NotificationsSelected),
    //            nameof(FeedsSelected),
    //            nameof(ListsSelected),
    //            nameof(ChatSelected),
    //            nameof(ProfileSelected));
    //}
}
