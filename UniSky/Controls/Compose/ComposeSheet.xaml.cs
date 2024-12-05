using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using UniSky.Controls.Sheet;
using UniSky.ViewModels.Compose;
using UniSky.ViewModels.Posts;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
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
        private readonly ResourceLoader strings;

        public ComposeViewModel ViewModel
        {
            get => (ComposeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ComposeViewModel), typeof(ComposeSheet), new PropertyMetadata(null));

        public ComposeSheet()
             : base()
        {
            this.InitializeComponent();
            this.Showing += OnShowing;
            this.Shown += OnShown;
            this.Hiding += OnHiding;
            this.Hidden += OnHidden;
            this.strings = ResourceLoader.GetForCurrentView();
        }

        public bool Not(bool b, bool a)
            => !a && !b;

        private void OnShowing(SheetControl sender, SheetShowingEventArgs e)
        {
            var inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += OnInputPaneShowing;
            inputPane.Hiding += OnInputPaneHiding;

            if (Window.Current.Content is FrameworkElement element)
            {
                element.AllowDrop = true;
                element.DragEnter += HandleDrag;
                element.DragOver += HandleDrag;
                element.DragLeave += HandleDrag;
                element.Drop += HandleDrop;
            }

            if (e.Parameter is PostViewModel replyTo)
            {
                this.ViewModel = ActivatorUtilities.CreateInstance<ComposeViewModel>(Ioc.Default, replyTo);
            }
            else
            {
                this.ViewModel = ActivatorUtilities.CreateInstance<ComposeViewModel>(Ioc.Default);
            }
        }

        private void OnHidden(SheetControl sender, RoutedEventArgs args)
        {
            var inputPane = InputPane.GetForCurrentView();
            inputPane.Showing -= OnInputPaneShowing;
            inputPane.Hiding -= OnInputPaneHiding;

            if (Window.Current.Content is FrameworkElement element)
            {
                element.AllowDrop = false;
                element.DragEnter -= HandleDrag;
                element.DragOver -= HandleDrag;
                element.DragLeave -= HandleDrag;
                element.Drop -= HandleDrop;
            }
        }

        private void OnShown(SheetControl sender, RoutedEventArgs args)
        {
            PrimaryTextBox.Focus(FocusState.Programmatic);
        }

        private async void OnHiding(SheetControl sender, SheetHidingEventArgs e)
        {
            var deferral = e.GetDeferral();
            try
            {
                if (ViewModel.IsDirty && await new ComposeDiscardDraftDialog().ShowAsync() != ContentDialogResult.Primary)
                {
                    e.Cancel = true;
                    return;
                }
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void OnInputPaneShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (ActualWidth > 620) return;

            ContentGrid.Padding = new Thickness(0, 0, 0, args.OccludedRect.Height);
            args.EnsuredFocusedElementInView = true;
        }

        private void OnInputPaneHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (ActualWidth > 620) return;

            ContentGrid.Padding = new Thickness(0, 0, 0, args.OccludedRect.Height);
            args.EnsuredFocusedElementInView = true;
        }

        private void HandleDrag(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Bitmap) ||
                e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = strings.GetString("UploadToBluesky");
                e.DragUIOverride.IsCaptionVisible = true;
            }
            else if (e.DataView.Contains(StandardDataFormats.Text) ||
                     e.DataView.Contains(StandardDataFormats.WebLink))
            {
                e.AcceptedOperation = DataPackageOperation.Link;
            }
        }

        private void HandleDrop(object sender, DragEventArgs e)
        {
            e.Handled = ViewModel.HandleDrop(e.DataView);
        }

        private void PrimaryTextBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            e.Handled = ViewModel.HandlePaste();
        }
    }
}
