using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using UniSky.Controls.Sheet;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace UniSky.Services;

internal class SheetService : ISheetService
{
    private readonly SheetRootControl sheetRoot;
    private readonly ISettingsService settingsService;

    public SheetService(ISettingsService settingsService)
    {
        this.settingsService = settingsService;
        this.sheetRoot = Window.Current.Content.FindDescendant<SheetRootControl>();
    }

    public async Task<ISheetController> ShowAsync<T>(object parameter = null) where T : SheetControl, new()
    {
        if (sheetRoot != null && !settingsService.Read("WindowedSheets", AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop"))
        {
            var safeArea = ServiceContainer.Scoped.GetRequiredService<ISafeAreaService>();

            var control = new T();
            var controller = new SheetRootController(sheetRoot, safeArea);

            control.SetSheetController(controller);

            sheetRoot.ShowSheet(control, parameter);
            return controller;
        }
        else
        {
            if (ApiInformation.IsApiContractPresent(typeof(UniversalApiContract).FullName, 8, 0))
            {
                return await ShowSheetForAppWindow<T>(parameter);
            }
            else
            {
                return await ShowSheetForCoreWindow<T>(parameter);
            }
        }
    }

    private async Task<ISheetController> ShowSheetForAppWindow<T>(object parameter) where T : SheetControl, new()
    {
        var control = new T();
        var appWindow = await AppWindow.TryCreateAsync();

        var controller = new AppWindowSheetController(appWindow, control);
        control.SetSheetController(controller);
        control.InvokeShowing(parameter);

        ElementCompositionPreview.SetAppWindowContent(appWindow, control);

        await appWindow.TryShowAsync();

        control.InvokeShown();

        return controller;
    }

    private static async Task<ISheetController> ShowSheetForCoreWindow<T>(object parameter) where T : SheetControl, new()
    {
        ISheetController controller = null;

        var currentViewId = ApplicationView.GetForCurrentView().Id;
        var view = CoreApplication.CreateNewView();
        var newViewId = 0;
        await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            newViewId = ApplicationView.GetForCurrentView().Id;

            var control = new T();
            controller = new ApplicationViewSheetController(control, currentViewId, newViewId);
            control.SetSheetController(controller);

            Window.Current.Content = control;
            Window.Current.Activate();

            control.InvokeShowing(parameter);
            control.InvokeShown();

        });

        await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId, ViewSizePreference.UseLess);

        return controller;
    }
}
