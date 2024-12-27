using System.Xml;
using System;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Crypto.Macs;
using UniSky.Helpers.Composition;
using UniSky.Pages;
using UniSky.Services;
using UniSky.ViewModels;
using UniSky.ViewModels.Profile;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UniSky;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RootPage : Page
{
    private SplashScreen splashScreen;
    private Rect splashScreenRect;
    private bool dismissed;

    public RootViewModel ViewModel
    {
        get => (RootViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register("ViewModel", typeof(RootViewModel), typeof(RootPage), new PropertyMetadata(null));

    public RootPage()
    {
        this.InitializeComponent();
        this.DataContext = this.ViewModel = ActivatorUtilities.CreateInstance<RootViewModel>(ServiceContainer.Scoped);
        //this.Loaded += RootPage_Loaded;
    }

    private void RootFrame_Navigated(object sender, NavigationEventArgs e)
    {
        Dismiss();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ServiceContainer.Scoped.GetRequiredService<ISafeAreaService>();

        var serviceLocator = ServiceContainer.Scoped.GetRequiredService<INavigationServiceLocator>();
        var service = serviceLocator.GetNavigationService("Root");
        service.Frame = RootFrame;

        if (e.Parameter is SplashScreen splashScreen)
        {
            Window.Current.SizeChanged += OnSizeChanged;

            this.splashScreen = splashScreen;
            this.splashScreenRect = splashScreen.ImageLocation;
            this.PositionImage();
            this.PositionRing();
        }
    }

    private void OnSizeChanged(object sender, WindowSizeChangedEventArgs e)
    {
        if (splashScreen != null)
        {
            // Update the coordinates of the splash screen image.
            splashScreenRect = splashScreen.ImageLocation;
            PositionImage();
            PositionRing();
        }
    }

    void PositionImage()
    {
        ExtendedSplashContainer.SetValue(Canvas.LeftProperty, splashScreenRect.X);
        ExtendedSplashContainer.SetValue(Canvas.TopProperty, splashScreenRect.Y);
        ExtendedSplashContainer.Height = splashScreenRect.Height;
        ExtendedSplashContainer.Width = splashScreenRect.Width;
    }

    void PositionRing()
    {
        ExtendedProgressRing.SetValue(Canvas.LeftProperty, splashScreenRect.X + (splashScreenRect.Width * 0.5) - (ExtendedProgressRing.Width * 0.5));
        ExtendedProgressRing.SetValue(Canvas.TopProperty, (splashScreenRect.Y + splashScreenRect.Height + splashScreenRect.Height * 0.1));
    }

    void Dismiss()
    {
        _ = Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
        {
            if (dismissed)
                return;

            dismissed = true;
            ExtendedProgressRing.IsActive = false;
            BirdAnimation.RunBirdAnimation(ExtendedSplash, SheetRoot);
        });
    }
}
