using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AurochWebInstaller
{
    public partial class InstallerWindow : Form
    {
        public static InstallerWindow SingleInstance;
        public InstallerWindow()
        {
            if (SingleInstance is null)
                SingleInstance = this;
            InitializeComponent();
        }

        public void LogBox(string Content, bool NewLine = true, bool NoSuffix = false)
        {
            WindowLogBox.AppendText((NoSuffix ? "" : "<> ") + Content + (NewLine ? Environment.NewLine : ""));
        }

        private void AurochWebsiteLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void InstallerWindow_Load(object sender, EventArgs e)
        {
            Text = "Auroch (" + InstallerVersion.GetVersionString() + ") — Web Installer";
            LogBox("Auroch Web Installer initialized.");
            if (InstallerFs.IsAurochInstalled())
                LogBox("Auroch is already installed.");
            else
                LogBox("Auroch is not installed. Press \"Install\" to begin installation of the latest version. Administrative rights are required to install Auroch due to its web protocol.");
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            InstallButton.Enabled = false;
            Installer.EasyInstall();
        }
    }
}
