using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using UniSky.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MUXC = Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UniSky.Controls.Sheet
{
    public sealed partial class SheetHostControl : UserControl
    {
        public SheetHostControl()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var safeAreaService = Ioc.Default.GetRequiredService<ISafeAreaService>();
            safeAreaService.SafeAreaUpdated += OnSafeAreaUpdated;
        }

        private void OnSafeAreaUpdated(object sender, SafeAreaUpdatedEventArgs e)
        {
            SheetScrollViewer.Padding = new Thickness(0, 16 + e.SafeArea.Bounds.Top, 0, 0);
            RootGrid.Height = ActualHeight - (SheetScrollViewer.Padding.Top + SheetScrollViewer.Padding.Bottom);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            RootGrid.Height = finalSize.Height - (SheetScrollViewer.Padding.Top + SheetScrollViewer.Padding.Bottom);

            return base.ArrangeOverride(finalSize);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.FindParent<SheetRootControl>()
                .HideSheet();
        }

        internal void Navigate(Type type, object parameter = null)
        {
            SheetContentFrame.Navigate(type, parameter);
        }

        private void RefreshContainer_RefreshRequested(MUXC.RefreshContainer sender, MUXC.RefreshRequestedEventArgs args)
        {
            this.FindParent<SheetRootControl>()
                .HideSheet();
        }
    }
}
