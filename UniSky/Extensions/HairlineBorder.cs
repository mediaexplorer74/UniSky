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

        private static List<WeakReference<DependencyObject>> Elements { get; set; } = [];
        private static DisplayInformation DisplayInfo { get; set; }

        private static void OnThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not (Grid or StackPanel or Control or Border))
                return;

            Elements.Add(new WeakReference<DependencyObject>(d));

            if (DisplayInfo == null)
            {
                Initialize();
            }

            var newValue = (Thickness)(e.NewValue);
            Apply(d, newValue, DisplayInfo);
        }

        private static void Apply(DependencyObject d, Thickness newValue, DisplayInformation info)
        {
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
                case StackPanel s:
                    s.BorderThickness = thickness;
                    break;
            }
        }

        private static void Initialize()
        {
            DisplayInfo = DisplayInformation.GetForCurrentView();
            DisplayInfo.DpiChanged += OnDpiChanged;
        }

        private static void OnDpiChanged(DisplayInformation sender, object args)
        {
            foreach (var item in Elements)
            {
                if (!item.TryGetTarget(out var element) || element.GetValue(ThicknessProperty) is not Thickness thickness)
                    continue;

                Apply(element, thickness, sender);
            }
        }
    }
}
