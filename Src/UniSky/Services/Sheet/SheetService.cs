﻿using System;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using UniSky.Controls.Sheet;
using UniSky.Services.Overlay;
using Windows.UI.Xaml;

namespace UniSky.Services;

internal class SheetService(ITypedSettings settingsService, ISafeAreaService safeAreaService) : OverlayService, ISheetService
{
    private readonly SheetRootControl sheetRoot = Window.Current.Content.FindDescendant<SheetRootControl>();

    public Task<IOverlayController> ShowAsync<T>(object parameter = null) where T : FrameworkElement, ISheetControl, new()
        => ShowAsync<T>(() => new T(), parameter);

    public async Task<IOverlayController> ShowAsync<T>(Func<T> factory, object parameter = null) where T : FrameworkElement, ISheetControl
    {
        if (sheetRoot == null || settingsService.UseMultipleWindows)
            return await ShowOverlayForWindow<T>(factory, parameter);

        var control = factory();
        var controller = new SheetRootController(sheetRoot, safeAreaService);

        control.SetOverlayController(controller);

        sheetRoot.ShowSheet(control, parameter);
        return controller;
    }
}
