using System.Threading;
using Synapse_UI_WPF.Interfaces;

namespace Synapse_UI_WPF.Static
{
    public static class Globals
    {
        public static string Version;
        public static string LauncherPath;
        public static string DllPath;
        public static ThemeInterface.TBase Theme;
        public static SynchronizationContext Context;
        public static Data.Options Options;
    }
}
