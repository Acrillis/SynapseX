using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AurochWebInstaller
{
    static class InstallerVersion
    {
        public static string Major = "1";
        public static string Minor = "0";
        public static string Extra = "β";

        public static string GetVersionString()
        {
            return Major + "." + Minor + Extra;
        }
    }
}
