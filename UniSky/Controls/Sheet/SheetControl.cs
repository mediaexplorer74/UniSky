using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Deferred;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using UniSky.Services;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.WindowManagement.Preview;
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
    public class SheetShowingEventArgs : RoutedEventArgs
    {
        public object Parameter { get; }

        public SheetShowingEventArgs(object parameter)
        {
            Parameter = parameter;
        }
    }

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

        public bool IsPrimaryButtonEnabled
        {
            get => (bool)GetValue(IsPrimaryButtonEnabledProperty);
            set => SetValue(IsPrimaryButtonEnabledProperty, value);
        }

        public static readonly DependencyProperty IsPrimaryButtonEnabledProperty =
            DependencyProperty.Register("IsPrimaryButtonEnabled", typeof(bool), typeof(SheetControl), new PropertyMetadata(true));

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

        public bool IsSecondaryButtonEnabled
        {
            get => (bool)GetValue(IsSecondaryButtonEnabledProperty);
            set => SetValue(IsSecondaryButtonEnabledProperty, value);
        }

        public static readonly DependencyProperty IsSecondaryButtonEnabledProperty =
            DependencyProperty.Register("IsSecondaryButtonEnabled", typeof(bool), typeof(SheetControl), new PropertyMetadata(true));

        public Size PreferredWindowSize
        {
            get { return (Size)GetValue(PreferredWindowSizeProperty); }
            set { SetValue(PreferredWindowSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PreferredWindowSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreferredWindowSizeProperty =
            DependencyProperty.Register("PreferredWindowSize", typeof(Size), typeof(SheetControl), new PropertyMetadata(new Size(320, 400)));


        public event TypedEventHandler<SheetControl, SheetShowingEventArgs> Showing;
        public event TypedEventHandler<SheetControl, RoutedEventArgs> Shown;
        public event TypedEventHandler<SheetControl, SheetHidingEventArgs> Hiding;
        public event TypedEventHandler<SheetControl, RoutedEventArgs> Hidden;

        public ISheetController Controller { get; private set; }

        public SheetControl()
        {
            this.DefaultStyleKey = typeof(SheetControl);
        }

        internal void SetSheetController(ISheetController controller)
        {
            Controller = controller;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Controller != null && Controller.IsFullWindow)
            {
                VisualStateManager.GoToState(this, "FullWindow", false);
                var titleBarDragArea = this.FindDescendantByName("TitleBarDragArea");
                Controller.SafeAreaService.SafeAreaUpdated += OnSafeAreaUpdated;
                Controller.SafeAreaService.SetTitleBar(titleBarDragArea);

                var inputPane = InputPane.GetForCurrentView();
                inputPane.Showing += OnInputPaneShowing;
                inputPane.Hiding += OnInputPaneHiding;

                this.SizeChanged += OnSizeChanged;
            }
            else
            {
                VisualStateManager.GoToState(this, "Standard", false);
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var rightButton = this.FindDescendantByName("PrimaryTitleBarButton");
            this.OnBottomInsetsChanged(0, rightButton.ActualWidth + 16);
        }

        internal void InvokeShowing(object parameter)
        {
            Showing?.Invoke(this, new SheetShowingEventArgs(parameter));
        }

        internal async Task<bool> InvokeHidingAsync()
        {
            var ev = new SheetHidingEventArgs();
            Hiding?.Invoke(this, ev);

            await ev.WaitOnDeferral();

            return !ev.Cancel;
        }

        internal void InvokeShown()
        {
            Shown?.Invoke(this, new RoutedEventArgs());
        }

        internal void InvokeHidden()
        {
            if (Controller.IsFullWindow)
            {
                Controller.SafeAreaService.SafeAreaUpdated += OnSafeAreaUpdated;
            }

            Hidden?.Invoke(this, new RoutedEventArgs());
        }

        private void OnInputPaneShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            var ButtonsGrid = (Grid)this.FindDescendantByName("ButtonsGrid");
            ButtonsGrid.Margin = new Thickness(0, 0, 0, args.OccludedRect.Height);
            args.EnsuredFocusedElementInView = true;
        }

        private void OnInputPaneHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            var ButtonsGrid = (Grid)this.FindDescendantByName("ButtonsGrid");
            ButtonsGrid.Margin = new Thickness(0, 0, 0, args.OccludedRect.Height);
            args.EnsuredFocusedElementInView = true;
        }

        private void OnSafeAreaUpdated(object sender, SafeAreaUpdatedEventArgs e)
        {
            var titleBarGrid = (Grid)this.FindDescendantByName("TitleBarGrid");

            if (e.SafeArea.HasTitleBar)
            {
                titleBarGrid.Height = e.SafeArea.Bounds.Top;
                titleBarGrid.Padding = new Thickness();
            }
            else
            {
                titleBarGrid.Height = 42;
                titleBarGrid.Padding = new Thickness(0, e.SafeArea.Bounds.Top, 0, 4);
            }

            Margin = new Thickness(e.SafeArea.Bounds.Left, 0, e.SafeArea.Bounds.Right, e.SafeArea.Bounds.Bottom);
        }

        protected virtual void OnBottomInsetsChanged(double leftInset, double rightInset) { }
    }
}
