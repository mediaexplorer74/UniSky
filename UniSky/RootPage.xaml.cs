using Microsoft.Extensions.DependencyInjection;
using UniSky.Helpers.Composition;
using UniSky.Pages;
using UniSky.Services;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UniSky;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RootPage : Page
{
    public RootPage()
    {
        this.InitializeComponent();
        Loaded += RootPage_Loaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var serviceLocator = ServiceContainer.Scoped.GetRequiredService<INavigationServiceLocator>();
        var service = serviceLocator.GetNavigationService("Root");
        service.Frame = RootFrame;

        var sessionService = ServiceContainer.Scoped.GetRequiredService<SessionService>();
        if (ApplicationData.Current.LocalSettings.Values.TryGetValue("LastUsedUser", out var userObj) &&
            userObj is string user)
        {
            service.Navigate<HomePage>(user);
        }
        else
        {
            service.Navigate<LoginPage>();
        }
    }

    private void RootPage_Loaded(object sender, RoutedEventArgs e)
    {
        BirdAnimation.RunBirdAnimation(SheetRoot);
    }
}
