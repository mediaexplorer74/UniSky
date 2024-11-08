using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.DependencyInjection;
using FishyFlip.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media;
using UniSky.Services;
using UniSky.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using MUXC = Microsoft.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.Foundation.Metadata;
using Windows.Phone;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UniSky.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomeViewModel ViewModel
        {
            get { return (HomeViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(HomeViewModel), typeof(HomePage), new PropertyMetadata(null));

        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApiInformation.IsApiContractPresent(typeof(PhoneContract).FullName, 1))
            {
                AppTitleBar.Visibility = Visibility.Collapsed;

                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = ((SolidColorBrush)Background).Color;
            }
            else
            {
                CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

                titleBar.ButtonBackgroundColor = Colors.Transparent;

                Window.Current.SetTitleBar(AppTitleBar);

                coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
                coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
                Window.Current.CoreWindow.Activated += CoreWindow_Activated;
            }

            var serviceLocator = Ioc.Default.GetRequiredService<INavigationServiceLocator>();
            var service = serviceLocator.GetNavigationService("Home");
            service.Frame = NavigationFrame;

            if (e.Parameter is string session || e.Parameter is ATDid did && (session = did.Handler) != null)
            {
                ViewModel = ActivatorUtilities.CreateInstance<HomeViewModel>(Ioc.Default, session);
            }
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            // Get the size of the caption controls and set padding.
            LeftPaddingColumn.Width = new GridLength(sender.SystemOverlayLeftInset);
            //RightPaddingColumn.Width = new GridLength(sender.SystemOverlayRightInset);
            AppTitleBar.Height = sender.Height;
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                VisualStateManager.GoToState(this, "Inactive", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Active", true);
            }
        }

        private void NavigationView_ItemInvoked(MUXC.NavigationView sender, MUXC.NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer.Tag is HomePages page)
                ViewModel.Page = page;
        }

        private void FooterToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            var page = (HomePages)button.Tag;

            ViewModel.Page = page;
        }

        private void FooterToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateChecked();
        }

        private void NavView_PaneOpening(MUXC.NavigationView sender, object args)
        {
            if (sender.PaneDisplayMode == MUXC.NavigationViewPaneDisplayMode.LeftCompact)
                PaneOpenStoryboard.Begin();
        }

        private void NavView_PaneClosing(MUXC.NavigationView sender, MUXC.NavigationViewPaneClosingEventArgs args)
        {
            if (sender.PaneDisplayMode == MUXC.NavigationViewPaneDisplayMode.LeftCompact)
                PaneCloseStoryboard.Begin();
        }
    }
}
