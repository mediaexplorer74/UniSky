using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Uwp.Deferred;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using UniSky.Services;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace UniSky.Controls.Sheet
{
    public class SheetHidingEventArgs : RoutedEventArgs
    {
        private Deferral _deferral;
        private TaskCompletionSource<object> _deferralCompletion;

        public Deferral GetDeferral()
        {
            _deferralCompletion = new TaskCompletionSource<object>();
            return (_deferral ??= new Deferral(OnDeferralCompleted));
        }

        public bool Cancel { get; set; } = false;

        internal Task WaitOnDeferral()
        {
            if (_deferral == null)
                return Task.CompletedTask;
            else
                return _deferralCompletion.Task;
        }

        private void OnDeferralCompleted()
        {
            _deferralCompletion?.SetResult(null);
        }
    }

    [ContentProperty(Name = nameof(SheetContent))]
    public class SheetControl : Control
    {
        public object SheetContent
        {
            get => (object)GetValue(SheetContentProperty);
            set => SetValue(SheetContentProperty, value);
        }

        public static readonly DependencyProperty SheetContentProperty =
            DependencyProperty.Register("SheetContent", typeof(object), typeof(SheetControl), new PropertyMetadata(null));

        public DataTemplate SheetContentTemplate
        {
            get => (DataTemplate)GetValue(SheetContentTemplateProperty);
            set => SetValue(SheetContentTemplateProperty, value);
        }

        public static readonly DependencyProperty SheetContentTemplateProperty =
            DependencyProperty.Register("SheetContentTemplate", typeof(DataTemplate), typeof(SheetControl), new PropertyMetadata(null));

        public object TitleContent
        {
            get => (object)GetValue(TitleContentProperty);
            set => SetValue(TitleContentProperty, value);
        }

        public static readonly DependencyProperty TitleContentProperty =
            DependencyProperty.Register("TitleContent", typeof(object), typeof(SheetControl), new PropertyMetadata(null));

        public DataTemplate TitleContentTemplate
        {
            get => (DataTemplate)GetValue(TitleContentTemplateProperty);
            set => SetValue(TitleContentTemplateProperty, value);
        }

        public static readonly DependencyProperty TitleContentTemplateProperty =
            DependencyProperty.Register("TitleContentTemplate", typeof(DataTemplate), typeof(SheetControl), new PropertyMetadata(null));

        public object PrimaryButtonContent
        {
            get => (object)GetValue(PrimaryButtonContentProperty);
            set => SetValue(PrimaryButtonContentProperty, value);
        }

        public static readonly DependencyProperty PrimaryButtonContentProperty =
            DependencyProperty.Register("PrimaryButtonContent", typeof(object), typeof(SheetControl), new PropertyMetadata(null));

        public DataTemplate PrimaryButtonContentTemplate
        {
            get => (DataTemplate)GetValue(PrimaryButtonContentTemplateProperty);
            set => SetValue(PrimaryButtonContentTemplateProperty, value);
        }

        public static readonly DependencyProperty PrimaryButtonContentTemplateProperty =
            DependencyProperty.Register("PrimaryButtonContentTemplate", typeof(DataTemplate), typeof(SheetControl), new PropertyMetadata(null));

        public Visibility PrimaryButtonVisibility
        {
            get => (Visibility)GetValue(PrimaryButtonVisibilityProperty);
            set => SetValue(PrimaryButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty PrimaryButtonVisibilityProperty =
            DependencyProperty.Register("PrimaryButtonVisibility", typeof(Visibility), typeof(SheetControl), new PropertyMetadata(Visibility.Visible));

        public ICommand PrimaryButtonCommand
        {
            get => (ICommand)GetValue(PrimaryButtonCommandProperty);
            set => SetValue(PrimaryButtonCommandProperty, value);
        }

        public static readonly DependencyProperty PrimaryButtonCommandProperty =
            DependencyProperty.Register("PrimaryButtonCommand", typeof(ICommand), typeof(SheetControl), new PropertyMetadata(null));

        public object SecondaryButtonContent
        {
            get => (object)GetValue(SecondaryButtonContentProperty);
            set => SetValue(SecondaryButtonContentProperty, value);
        }

        public static readonly DependencyProperty SecondaryButtonContentProperty =
            DependencyProperty.Register("SecondaryButtonContent", typeof(object), typeof(SheetControl), new PropertyMetadata(null));

        public DataTemplate SecondaryButtonContentTemplate
        {
            get => (DataTemplate)GetValue(SecondaryButtonContentTemplateProperty);
            set => SetValue(SecondaryButtonContentTemplateProperty, value);
        }

        public static readonly DependencyProperty SecondaryButtonContentTemplateProperty =
            DependencyProperty.Register("SecondaryButtonContentTemplate", typeof(DataTemplate), typeof(SheetControl), new PropertyMetadata(null));

        public Visibility SecondaryButtonVisibility
        {
            get => (Visibility)GetValue(SecondaryButtonVisibilityProperty);
            set => SetValue(SecondaryButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty SecondaryButtonVisibilityProperty =
            DependencyProperty.Register("SecondaryButtonVisibility", typeof(Visibility), typeof(SheetControl), new PropertyMetadata(Visibility.Visible));

        public ICommand SecondaryButtonCommand
        {
            get => (ICommand)GetValue(SecondaryButtonCommandProperty);
            set => SetValue(SecondaryButtonCommandProperty, value);
        }

        public static readonly DependencyProperty SecondaryButtonCommandProperty =
            DependencyProperty.Register("SecondaryButtonCommand", typeof(ICommand), typeof(SheetControl), new PropertyMetadata(null));

        public event TypedEventHandler<SheetControl, RoutedEventArgs> Showing;
        public event TypedEventHandler<SheetControl, SheetHidingEventArgs> Hiding;

        public SheetControl()
        {
            this.DefaultStyleKey = typeof(SheetControl);
            this.Loaded += OnLoaded;
        }

        internal void InvokeShowing()
        {
            Showing?.Invoke(this, new RoutedEventArgs());
        }

        internal async Task<bool> InvokeHidingAsync()
        {
            var ev = new SheetHidingEventArgs();
            Hiding?.Invoke(this, ev);

            await ev.WaitOnDeferral();

            return !ev.Cancel;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!DesignMode.DesignModeEnabled)
            {
                var safeAreaService = Ioc.Default.GetService<ISafeAreaService>();
                if (safeAreaService != null)
                    safeAreaService.SafeAreaUpdated += OnSafeAreaUpdated;
            }
        }

        private void OnSafeAreaUpdated(object sender, SafeAreaUpdatedEventArgs e)
        {
            var sheetScrollViewer = (ScrollViewer)this.FindDescendantByName("SheetScrollViewer");
            var rootGrid = (Grid)this.FindDescendantByName("RootGrid");

            if (rootGrid != null && sheetScrollViewer != null)
            {
                sheetScrollViewer.Padding = new Thickness(0, 16 + e.SafeArea.Bounds.Top, 0, 0);
                rootGrid.Height = Math.Max(0, ActualHeight - (sheetScrollViewer.Padding.Top + sheetScrollViewer.Padding.Bottom));
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.ApplyTemplate();

            var sheetScrollViewer = (ScrollViewer)this.FindDescendantByName("SheetScrollViewer");
            var rootGrid = (Grid)this.FindDescendantByName("RootGrid");

            if (rootGrid != null && sheetScrollViewer != null)
                rootGrid.Height = finalSize.Height - (sheetScrollViewer.Padding.Top + sheetScrollViewer.Padding.Bottom);

            return base.ArrangeOverride(finalSize);
        }
    }
}
