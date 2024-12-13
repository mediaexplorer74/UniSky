using Microsoft.Extensions.DependencyInjection;
using UniSky.Services;
using UniSky.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace UniSky.Pages;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel
    {
        get => (LoginViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register("ViewModel", typeof(LoginViewModel), typeof(LoginPage), new PropertyMetadata(null));

    public LoginPage()
    {
        this.InitializeComponent();
        this.ViewModel = ActivatorUtilities.CreateInstance<LoginViewModel>(ServiceContainer.Scoped);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        this.Frame.BackStack.Clear();
    }

    public bool IsNotNull(object o) 
        => o is not null;

    public bool Is(bool b)
        => b;
}
