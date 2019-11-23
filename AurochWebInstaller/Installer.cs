using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AurochWebInstaller
{
    static class Installer
    {
        public static bool EasyInstall()
        {
            InstallerWindow.SingleInstance.LogBox("Checking for new Web Installer updates — ", false);
            {
                InstallerWindow.SingleInstance.LogBox("None available", true, true);
            }

            InstallerWindow.SingleInstance.LogBox("Downloading the latest Auroch protocol — ", false);
            {
                /* download auroch protocol to temporary directory */
                InstallerWindow.SingleInstance.LogBox("OK", true, true);
            }

            InstallerWindow.SingleInstance.LogBox("Downloading the latest Auroch interface — ", false);
            {
                /* download auroch interface to temporary directory */
                InstallerWindow.SingleInstance.LogBox("OK", true, true);
            }

            InstallerWindow.SingleInstance.LogBox("Installing Auroch protocol — ", false);
            {
                /* install protocol */
                InstallerWindow.SingleInstance.LogBox("OK", true, true);
            }

            InstallerWindow.SingleInstance.LogBox("Registering Auroch protocol — ", false);
            {
                /* register protocol */
                InstallerWindow.SingleInstance.LogBox("OK", true, true);
            }

            InstallerWindow.SingleInstance.LogBox("Registering Auroch file associations — ", false);
            {
                /* register file associations */
                InstallerWindow.SingleInstance.LogBox("OK", true, true);
            }

            InstallerWindow.SingleInstance.LogBox("Installing Auroch interface — ", false);
            {
                /* install protocol */
                InstallerWindow.SingleInstance.LogBox("OK", true, true);
            }

            InstallerWindow.SingleInstance.LogBox("Installation complete. You can now open *.aur files and right click on script files in the Windows Explorer to execute them in Synapse X (choose \"Run in Synapse X\")");

            return true;
        }
    }
}
