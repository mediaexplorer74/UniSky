using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UniSky.Pages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UniSky.Controls.Sheet
{
    [ContentProperty(Name = nameof(ContentElement))]
    public sealed partial class SheetRootControl : UserControl
    {
        public SheetRootControl()
        {
            this.InitializeComponent();
        } 

        public FrameworkElement ContentElement
        {
            get { return (FrameworkElement)GetValue(ContentElementProperty); }
            set { SetValue(ContentElementProperty, value); }
        }

        public static readonly DependencyProperty ContentElementProperty =
            DependencyProperty.Register("ContentElement", typeof(FrameworkElement), typeof(SheetRootControl), new PropertyMetadata(null));

        protected override Size ArrangeOverride(Size finalSize)
        {
            // TODO: this depends on the state
            SheetTransform.Y = finalSize.Height;
            return base.ArrangeOverride(finalSize);
        }

        internal void ShowSheet(Type pageType, object parameter = null)
        {
            SheetTransform.Y = ActualHeight;
            ShowSheetStoryboard.Begin();

            HostControl.Navigate(pageType, parameter);
        }

        internal void HideSheet()
        {
            HideDoubleAnimation.To = ActualHeight;
            HideSheetStoryboard.Begin();
        }
    }
}
