using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace UniSky.Services;

public interface ITypedSettings
{
    ElementTheme RequestedColourScheme { get; set; }

    bool UseMultipleWindows { get; set; }

    bool AutoRefreshFeeds { get; set; }
}
