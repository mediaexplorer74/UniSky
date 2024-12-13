using System.Threading.Tasks;
using UniSky.Controls.Sheet;
using Windows.UI.Xaml;

namespace UniSky.Services;

public class SheetRootController(SheetRootControl rootControl,
                                 ISafeAreaService safeAreaService) : ISheetController
{
    public UIElement Root => rootControl;
    public bool IsFullWindow => false;
    public ISafeAreaService SafeAreaService => safeAreaService;

    public async Task<bool> TryHideSheetAsync()
    {
        return await rootControl.HideSheetAsync();
    }
}
