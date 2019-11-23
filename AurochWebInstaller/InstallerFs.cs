using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AurochWebInstaller
{
    static class InstallerFs
    {
        public static bool IsAurochInstalled()
        {
            if (Registry.LocalMachine.OpenSubKey("SOFTWARE\\Auroch", false) != null)
                return true;
            return false;
        }
    }
}
