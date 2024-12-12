using Microsoft.Toolkit.Uwp.Helpers;
using UniSky.Services;
using Windows.UI.Xaml;

using static UniSky.Constants.Settings;

namespace UniSky.ViewModels.Settings;

public class SettingsViewModel(ITypedSettings settingsService, IThemeService themeService) : ViewModelBase
{
    private readonly int _initialColour = (int)settingsService.RequestedColourScheme;
    private readonly int _initialTheme = (int)themeService.GetThemeForDisplay();

    public bool SunValleyThemeSupported
        => SystemInformation.OperatingSystemVersion.Build >= 17763;

    public int ColourScheme
    {
        get => (int)settingsService.RequestedColourScheme;
        set
        {
            settingsService.RequestedColourScheme = (ElementTheme)value;
            OnPropertyChanged(nameof(ColourScheme));
            OnPropertyChanged(nameof(IsDirty));
        }
    }

    public int ApplicationTheme
    {
        get => (int)themeService.GetThemeForDisplay();
        set
        {
            themeService.SetThemeOnRelaunch((AppTheme)value);
            OnPropertyChanged(nameof(ApplicationTheme));
            OnPropertyChanged(nameof(IsDirty));
        }
    }

    public bool UseMultipleWindows
    {
        get => settingsService.UseMultipleWindows;
        set => settingsService.UseMultipleWindows = value;
    }

    public bool AutoRefreshFeeds
    {
        get => settingsService.AutoRefreshFeeds;
        set => settingsService.AutoRefreshFeeds = value;
    }

    public bool IsDirty
        => ApplicationTheme != _initialTheme || ColourScheme != _initialColour;
}