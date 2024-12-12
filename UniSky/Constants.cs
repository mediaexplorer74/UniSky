using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UniSky.Models;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace UniSky;

public static class Constants
{
    public static string Version
    {
        get
        {
            var gitSha = "";
            var versionedAssembly = typeof(LoginModel).Assembly;
            var attribute = versionedAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            int idx;
            if (attribute != null && (idx = attribute.InformationalVersion.IndexOf('+')) != -1)
            {
                gitSha = "-" + attribute.InformationalVersion.Substring(idx + 1);
            }

            var v = Package.Current.Id.Version;
            return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}{gitSha}";
        }
    }

    public static string UserAgent
        => $"UniSky/{Version} (https://github.com/UnicordDev/UniSky)";

    public static class Settings
    {
        public const string REQUESTED_COLOUR_SCHEME = "RequestedColourScheme_v1";
        public const int REQUESTED_COLOUR_SCHEME_DEFAULT = (int)ElementTheme.Default;

        public const string USE_MULTIPLE_WINDOWS = "UseMultipleWindows_v1";
        // default: calculated

        public const string AUTO_FEED_REFRESH = "AutoRefreshFeeds_v1";
        public const bool AUTO_FEED_REFRESH_DEFAULT = true;
    }
}
