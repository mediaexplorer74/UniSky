using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using UniSky.Controls.Sheet;
using UniSky.ViewModels.Compose;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UniSky.Controls.Compose
{
    public sealed partial class ComposeSheet : SheetControl
    {
        public ComposeViewModel ViewModel
        {
            get { return (ComposeViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ComposeViewModel), typeof(ComposeSheet), new PropertyMetadata(null));

        public ComposeSheet()
             : base()
        {
            this.InitializeComponent();
            this.ViewModel = ActivatorUtilities.CreateInstance<ComposeViewModel>(Ioc.Default);
            this.Showing += OnShowing;
            this.Hiding += OnHiding;
        }

        private void OnShowing(SheetControl sender, RoutedEventArgs e)
        {
            var inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += OnInputPaneShowing;
            inputPane.Hiding += OnInputPaneHiding;
        }

        private async void OnHiding(SheetControl sender, SheetHidingEventArgs e)
        {
            var deferral = e.GetDeferral();
            try
            {
                var dialog = new MessageDialog("The current post will be lost!", "Discard post?");
                var yesCommand = new UICommand("Yes");
                var noCommand = new UICommand("No");
                dialog.Commands.Add(yesCommand);
                dialog.Commands.Add(noCommand);

                if ((await dialog.ShowAsync()) == noCommand)
                {
                    e.Cancel = true;
                    return;
                }

                var inputPane = InputPane.GetForCurrentView();
                inputPane.Showing -= OnInputPaneShowing;
                inputPane.Hiding -= OnInputPaneHiding;
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void OnInputPaneShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            ContentGrid.Padding = new Thickness(0, 0, 0, args.OccludedRect.Top - args.OccludedRect.Bottom);
            args.EnsuredFocusedElementInView = true;
        }

        private void OnInputPaneHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            ContentGrid.Padding = new Thickness(0, 0, 0, args.OccludedRect.Top - args.OccludedRect.Bottom);
            args.EnsuredFocusedElementInView = true;
        }

        public bool Not(bool b, bool a)
            => !a && !b;
    }
}
