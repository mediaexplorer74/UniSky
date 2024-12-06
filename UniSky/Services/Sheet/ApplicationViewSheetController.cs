using System;
using System.Threading.Tasks;
using UniSky.Controls.Sheet;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace UniSky.Services;

internal class ApplicationViewSheetController(SheetControl control,
                                              int hostViewId,
                                              int viewId,
                                              ISafeAreaService safeAreaService) : ISheetController
{
    public UIElement Root => control;
    public bool IsFullWindow => true;
    public ISafeAreaService SafeAreaService => safeAreaService;

    public async Task<bool> TryHideSheetAsync()
    {
        if (await control.InvokeHidingAsync())
        {
            await ApplicationViewSwitcher.SwitchAsync(hostViewId, viewId, ApplicationViewSwitchingOptions.ConsolidateViews);
            return true;
        }

        return false;
    }
}
