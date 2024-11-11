using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using UniSky.ViewModels;
using UniSky.ViewModels.Feeds;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Phone;
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
using UniSky.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UniSky.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FeedsPage : Page
    {
        public FeedsViewModel ViewModel
        {
            get { return (FeedsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(FeedsViewModel), typeof(FeedsPage), new PropertyMetadata(null));

        public FeedsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var safeAreaService = Ioc.Default.GetRequiredService<ISafeAreaService>();
            safeAreaService.SafeAreaUpdated += OnSafeAreaUpdated;

            this.ViewModel = ActivatorUtilities.CreateInstance<FeedsViewModel>(Ioc.Default);
        }

        private void OnSafeAreaUpdated(object sender, SafeAreaUpdatedEventArgs e)
        {
            RootGrid.Padding = new Thickness(0, e.SafeArea.Bounds.Top, 0, 0);
        }

        private async void OnRefreshRequested(MUXC.RefreshContainer sender, MUXC.RefreshRequestedEventArgs args)
        {
            var feed = (FeedViewModel)sender.DataContext;

            var deferral = args.GetDeferral();
            await feed.RefreshAsync(deferral);
        }
    }
}
