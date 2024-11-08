using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Graphics.Display;

namespace UniSky.Extensions
{
    public class HairlineBorder
    {
        public static Thickness GetThickness(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(ThicknessProperty);
        }

        public static void SetThickness(DependencyObject obj, Thickness value)
        {
            obj.SetValue(ThicknessProperty, value);
        }

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.RegisterAttached("Thickness", typeof(Thickness), typeof(HairlineBorder), new PropertyMetadata(new Thickness(), OnThicknessPropertyChanged));

        private static void OnThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not (Grid or Panel or Control))
                return;

            var newValue = (Thickness)(e.NewValue);

            // TODO: this should be faster
            var info = DisplayInformation.GetForCurrentView();
            var hairlineThickness = (1.0 / (info.LogicalDpi / 96.0));
            var thickness = new Thickness(
                newValue.Left != 0 ? hairlineThickness : 0,
                newValue.Top != 0 ? hairlineThickness : 0,
                newValue.Right != 0 ? hairlineThickness : 0,
                newValue.Bottom != 0 ? hairlineThickness : 0);

            switch (d)
            {
                case Grid g:
                    g.BorderThickness = thickness;
                    break;
                case Border p:
                    p.BorderThickness = thickness;
                    break;
                case Control c:
                    c.BorderThickness = thickness;
                    break;
            }
        }
    }
}
