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
        var settingsKey = "AppWindow_LastSize_" + typeof(T).FullName.Replace(".", "_");
        var initialSize = settingsService.Read(settingsKey, new Size(320, 400));

        var control = new T();
        var appWindow = await AppWindow.TryCreateAsync();
        var controller = new AppWindowSheetController(appWindow, control);
        control.SetSheetController(controller);
        control.InvokeShowing(parameter);

        ElementCompositionPreview.SetAppWindowContent(appWindow, control);

        appWindow.PersistedStateId = settingsKey;
        appWindow.CloseRequested += async (o, e) =>
        {
            var deferral = e.GetDeferral();
            if (!await control.InvokeHidingAsync())
                e.Cancel = true;

            deferral.Complete();
        };

        appWindow.Changed += (o, e) =>
        {
            if (e.DidSizeChange)
                settingsService.Save(settingsKey, new Size(control.ActualSize.X, control.ActualSize.Y));
        };

        appWindow.Closed += (o, e) =>
        {
            control.InvokeHidden();
        };
        
        appWindow.RequestSize(initialSize);

        var applicationView = ApplicationView.GetForCurrentView();
        var currentViewRect = applicationView.VisibleBounds;
        var environment = applicationView.WindowingEnvironment;
        if (environment.Kind == WindowingEnvironmentKind.Overlapped)
        {
            var regions = environment.GetDisplayRegions();
            var currentRegion = regions[0];
            foreach (var region in regions)
            {
                var regionRect = new Rect(region.WorkAreaOffset, region.WorkAreaSize);
                if (regionRect.Contains(new Point(applicationView.VisibleBounds.X, applicationView.VisibleBounds.Y)))
                    currentRegion = region;
            }

            var currentDisplayOffset = currentRegion.WorkAreaOffset;
            var currentDisplaySize = currentRegion.WorkAreaSize;
            currentViewRect = new Rect(
                currentViewRect.X - currentDisplayOffset.X,
                currentViewRect.Y - currentDisplayOffset.Y,
                currentViewRect.Width,
                currentViewRect.Height);

            var currentDisplayCenter = currentDisplaySize.Width / 2;
            var offset = (currentViewRect.Left + Math.Max(currentViewRect.Width / 2, initialSize.Width / 2)) - currentDisplayCenter;

            if (applicationView.AdjacentToLeftDisplayEdge && applicationView.AdjacentToRightDisplayEdge)
            {
                appWindow.RequestMoveRelativeToDisplayRegion(currentRegion, new Point((currentDisplayCenter - (initialSize.Width / 2)) + 20, 150));
            }
            else if (offset < 0)
            {
                // right
                appWindow.RequestMoveRelativeToCurrentViewContent(new Point(applicationView.VisibleBounds.Width + 8, 0));
            }
            else
            {
                // left
                appWindow.RequestMoveRelativeToCurrentViewContent(new Point(-initialSize.Width - 8, 0));
            }
        }

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

            // a surprise tool that'll help us later
            // (instanciating this now so it handles min. window sizes, etc.)
            var safeAreaService = ServiceContainer.Scoped.GetRequiredService<ISafeAreaService>();

            var control = new T();
            controller = new ApplicationViewSheetController(control, currentViewId, newViewId, safeAreaService);
            control.SetSheetController(controller);

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += async (o, e) =>
            {
                var deferral = e.GetDeferral();
                if (!await control.InvokeHidingAsync())
                    e.Handled = true;

                deferral.Complete();
            };

            Window.Current.Closed += (o, ev) =>
            {
                control.InvokeHidden();
            };

            Window.Current.Content = control;
            Window.Current.Activate();

            control.InvokeShowing(parameter);
            control.InvokeShown();

        });

        await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId, ViewSizePreference.UseLess);

        return controller;
    }
}
