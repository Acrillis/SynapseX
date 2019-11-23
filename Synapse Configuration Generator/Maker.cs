using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Synapse_Configuration_Generator
{
    public partial class Maker : Form
    {
        public Maker()
        {
            InitializeComponent();
        }

        [Serializable]
        public class SynUiData
        {
            public string DllDownload;
            public string DllHash;
            public string BetaDllDownload;
            public string BetaDllHash;
            public string BetaUiDownload;
            public string BetaUiHash;
            public string SxLibDownload;
            public string SxLibHash;
            public string SxLibXmlDownload;
            public string SxLibXmlHash;
            public string CefSharpDownload;
            public string CefSharpHash;
            public string LauncherDownload;
            public string LauncherHash;
            public string DiscordInvite;
            public string Version;
            public string UiVersion;
            public bool IsUpdated;
        }

        [Serializable]
        public class SynBootstrapperData
        {
            public string UiDownload;
            public string UiHash;
            public string InjectorDownload;
            public string InjectorHash;
            public string BootstrapperVersion;
        }

        [Serializable]
        public class SynScriptHubEntry
        {
            public string Name;
            public string Description;
            public string Picture;
            public string Url;
        }

        [Serializable]
        public class SynWhitelistWebSocketEntry
        {
            public string Origin;
            public string AppName;
            public string DevName;
        }

        [Serializable]
        public class SynWhitelistWebSocket
        {
            public List<string> EntriesNoPrompt;
            public List<SynWhitelistWebSocketEntry> EntriesPrompt;
        }

        [Serializable]
        public class SynScriptHubData
        {
            public List<SynScriptHubEntry> Entries = new List<SynScriptHubEntry>();
        }

        [Serializable]
        public class SynPaths
        {
            public string UiPath;
            public string BetaDllPath;
            public string BetaUiPath;
            public string CefSharpPath;
            public string SxLibPath;
            public string SxLibXmlPath;
            public string LauncherPath;
            public string DllPath;
            public string InjectorPath;
        }

        public static string Sha512(string Input, bool IsFile = false)
        {
            var bytes = IsFile ? File.ReadAllBytes(Input) : Encoding.ASCII.GetBytes(Input);
            using (var hash = SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);
                var hashedInputStringBuilder = new StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }

        public void SaveSettings()
        {
            var Bootstrap = new SynBootstrapperData
            {
                UiDownload = UIDownloadBox.Text,
                UiHash = Sha512(UIPathBox.Text, true),
                InjectorDownload = InjectorDownloadBox.Text,
                InjectorHash = Sha512(InjectorPathBox.Text, true),
                BootstrapperVersion = BootstrapperVersionBox.Text
            };

            var Ui = new SynUiData
            {
                DllDownload = DllDownloadBox.Text,
                DllHash = Sha512(DllPathBox.Text, true),
                BetaDllDownload = BetaDllDownloadBox.Text,
                BetaDllHash = Sha512(BetaDllPathBox.Text, true),
                BetaUiDownload = BetaUiDownloadBox.Text,
                BetaUiHash = Sha512(BetaUiPathBox.Text, true),
                CefSharpDownload = CefSharpDownloadBox.Text,
                CefSharpHash = Sha512(CefSharpPathBox.Text, true),
                SxLibDownload = SxLibDownloadBox.Text,
                SxLibHash = Sha512(SxLibPathBox.Text, true),
                SxLibXmlDownload = SxLibXmlDownloadBox.Text,
                SxLibXmlHash = Sha512(SxLibXmlPathBox.Text, true),
                LauncherDownload = LauncherDownloadBox.Text,
                LauncherHash = Sha512(LauncherPathBox.Text, true),
                DiscordInvite = DiscordInviteBox.Text,
                Version = VersionBox.Text,
                UiVersion = UiVersionBox.Text,
                IsUpdated = IsUpdatedBox.Checked
            };

            var Paths = new SynPaths
            {
                UiPath = UIPathBox.Text,
                InjectorPath = InjectorPathBox.Text,
                DllPath = DllPathBox.Text,
                BetaDllPath = BetaDllPathBox.Text,
                BetaUiPath = BetaUiPathBox.Text,
                CefSharpPath = CefSharpPathBox.Text,
                SxLibPath = SxLibPathBox.Text,
                SxLibXmlPath = SxLibXmlPathBox.Text,
                LauncherPath = LauncherPathBox.Text
            };

            File.WriteAllText("bootstrap.json", JsonConvert.SerializeObject(Bootstrap));
            File.WriteAllText("ui.json", JsonConvert.SerializeObject(Ui));
            File.WriteAllText("paths.json", JsonConvert.SerializeObject(Paths));
        }

        public void LoadSettings()
        {
            if (!File.Exists("bootstrap.json") || !File.Exists("ui.json") || !File.Exists("paths.json")) return;

            var Bootstrap = JsonConvert.DeserializeObject<SynBootstrapperData>(File.ReadAllText("bootstrap.json"));
            var Ui = JsonConvert.DeserializeObject<SynUiData>(File.ReadAllText("ui.json"));
            var Paths = JsonConvert.DeserializeObject<SynPaths>(File.ReadAllText("paths.json"));

            UIDownloadBox.Text = Bootstrap.UiDownload;
            UIPathBox.Text = Paths.UiPath;
            InjectorDownloadBox.Text = Bootstrap.InjectorDownload;
            InjectorPathBox.Text = Paths.InjectorPath;
            BootstrapperVersionBox.Text = Bootstrap.BootstrapperVersion;

            DllDownloadBox.Text = Ui.DllDownload;
            DllPathBox.Text = Paths.DllPath;
            BetaDllDownloadBox.Text = Ui.BetaDllDownload;
            BetaDllPathBox.Text = Paths.BetaDllPath;
            BetaUiDownloadBox.Text = Ui.BetaUiDownload;
            BetaUiPathBox.Text = Paths.BetaUiPath;
            CefSharpDownloadBox.Text = Ui.CefSharpDownload;
            CefSharpPathBox.Text = Paths.CefSharpPath;
            SxLibDownloadBox.Text = Ui.SxLibDownload;
            SxLibPathBox.Text = Paths.SxLibPath;
            SxLibXmlDownloadBox.Text = Ui.SxLibXmlDownload;
            SxLibXmlPathBox.Text = Paths.SxLibXmlPath;
            LauncherDownloadBox.Text = Ui.LauncherDownload;
            LauncherPathBox.Text = Paths.LauncherPath;
            DiscordInviteBox.Text = Ui.DiscordInvite;
            VersionBox.Text = Ui.Version;
            UiVersionBox.Text = Ui.UiVersion;
            IsUpdatedBox.Checked = Ui.IsUpdated;
        }

        private void Maker_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void CompileBootstrapBox_Click(object sender, EventArgs e)
        {
            var Bootstrap = new SynBootstrapperData
            {
                UiDownload = UIDownloadBox.Text,
                UiHash = Sha512(UIPathBox.Text, true),
                InjectorDownload = InjectorDownloadBox.Text,
                InjectorHash = Sha512(InjectorPathBox.Text, true),
                BootstrapperVersion = BootstrapperVersionBox.Text
            };

            var Comp = JsonConvert.SerializeObject(Bootstrap);
            Clipboard.SetText(Comp);
            BootstrapOutputBox.Text = Comp;

            SaveSettings();

            MessageBox.Show("Complete!");
        }

        private void UiCompileButton_Click(object sender, EventArgs e)
        {
            var Ui = new SynUiData
            {
                DllDownload = DllDownloadBox.Text,
                DllHash = Sha512(DllPathBox.Text, true),
                BetaDllDownload = BetaDllDownloadBox.Text,
                BetaDllHash = Sha512(BetaDllPathBox.Text, true),
                BetaUiDownload = BetaUiDownloadBox.Text,
                BetaUiHash = Sha512(BetaUiPathBox.Text, true),
                SxLibDownload = SxLibDownloadBox.Text,
                SxLibHash = Sha512(SxLibPathBox.Text, true),
                SxLibXmlDownload = SxLibXmlDownloadBox.Text,
                SxLibXmlHash = Sha512(SxLibXmlPathBox.Text, true),
                CefSharpDownload = CefSharpDownloadBox.Text,
                CefSharpHash = Sha512(CefSharpPathBox.Text, true),
                LauncherDownload = LauncherDownloadBox.Text,
                LauncherHash = Sha512(LauncherPathBox.Text, true),
                DiscordInvite = DiscordInviteBox.Text,
                Version = VersionBox.Text,
                UiVersion = UiVersionBox.Text,
                IsUpdated = IsUpdatedBox.Checked
            };

            var Comp = JsonConvert.SerializeObject(Ui);
            Clipboard.SetText(Comp);
            UiOutputBox.Text = Comp;

            SaveSettings();

            MessageBox.Show("Complete!");
        }

        private void CompileWebSocketButton_Click(object sender, EventArgs e)
        {
            WebSocketBox.Text = JsonConvert.SerializeObject(new SynWhitelistWebSocket
            {
                EntriesNoPrompt = new List<string>
                {
                    UrlWebSocketBox.Text
                },
                EntriesPrompt = new List<SynWhitelistWebSocketEntry>
                {
                    new SynWhitelistWebSocketEntry
                    {
                        AppName = "Synapse X",
                        DevName = "3dsboy08",
                        Origin = "https://3dsboy08.github.io"
                    },
                    new SynWhitelistWebSocketEntry
                    {
                        AppName = "Synapse X Offical",
                        DevName = "3dsboy08",
                        Origin = "https://loukamb.github.io"
                    }
                }
            }, Formatting.Indented);

            MessageBox.Show("Complete!");
        }
    }
}
