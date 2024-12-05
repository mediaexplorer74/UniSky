using System;
using System.Threading.Tasks;
using UniSky.Controls.Sheet;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;

namespace UniSky.Services;

public class AppWindowSheetController(AppWindow appWindow,
                                      SheetControl control) : ISheetController
{
    public UIElement Root => control;
    public bool IsFullWindow => true;
    public ISafeAreaService SafeAreaService { get; } = new AppWindowSafeAreaService(appWindow);

    public async Task<bool> TryHideSheetAsync()
    {
        if (await control.InvokeHidingAsync())
        {
            await appWindow.CloseAsync();
            return true;
        }

        return false;
    }
}
