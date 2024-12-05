using System;
using Windows.UI.Xaml;

namespace UniSky.Services
{
    internal interface ISafeAreaService
    {
        SafeAreaInfo State { get; }
        event EventHandler<SafeAreaUpdatedEventArgs> SafeAreaUpdated;

        void SetTitlebarTheme(ElementTheme theme);
    }
}