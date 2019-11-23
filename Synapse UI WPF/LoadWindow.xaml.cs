#define USE_UPDATE_CHECKS
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using Synapse_UI_WPF.Interfaces;
using Synapse_UI_WPF.Static;

namespace Synapse_UI_WPF
{
    public partial class LoadWindow
    {
        public const string UiVersion = "14";
        public const uint TVersion = 2;

        public static ThemeInterface.TInitStrings InitStrings;
        public static BackgroundWorker LoadWorker = new BackgroundWorker();

        public LoadWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var Result = MessageBox.Show(
                    $"Synapse has encountered an exception. Please report the following text below to 3dsboy08 on Discord (make sure to give the text, not an image):\n\n{((Exception)args.ExceptionObject)}\n\nIf you would like this text copied to your clipboard, press \'Yes\'.",
                    "Synapse X",
                    MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No);

                if (Result != MessageBoxResult.Yes) return;

                var STAThread = new Thread(
                    delegate ()
                    {
                        Clipboard.SetText(((Exception)args.ExceptionObject).ToString());
                    });

                STAThread.SetApartmentState(ApartmentState.STA);
                STAThread.Start();
                STAThread.Join();

                Thread.Sleep(1000);
            };

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            LoadWorker.DoWork += LoadWorker_DoWork;

            InitializeComponent();
        }

        public void SetStatusText(string Status, int Percentage)
        {
            Dispatcher.Invoke(() =>
            {
                StatusBox.Content = Status;
                ProgressBox.Value = Percentage;
            });
        }

        private ThemeInterface.TBase MigrateT1ToT2(ThemeInterface.TBase Old)
        {
            Old.Version = 2;

            Old.Main.ExecuteFileButton = new ThemeInterface.TButton
            {
                BackColor = new ThemeInterface.TColor(255, 60, 60, 60),
                TextColor = new ThemeInterface.TColor(255, 255, 255, 255),
                Font = new ThemeInterface.TFont("Segoe UI", 14f),
                Image = new ThemeInterface.TImage(),
                Text = "Execute File"
            };

            return Old;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var ProcList = Process.GetProcessesByName(
                Path.GetFileName(AppDomain.CurrentDomain.FriendlyName));
            var Current = Process.GetCurrentProcess();
            foreach (var Proc in ProcList)
            {
                if (Proc.Id == Current.Id) continue;
                try
                {
                    Proc.Kill();
                }
                catch (Exception)
                {
                }
            }

            if (!File.Exists("bin\\theme-wpf.json"))
            {
                File.WriteAllText("bin\\theme-wpf.json",
                    JsonConvert.SerializeObject(ThemeInterface.Default(), Formatting.Indented));
            }

            try
            {
                Globals.Theme =
                    JsonConvert.DeserializeObject<ThemeInterface.TBase>(File.ReadAllText("bin\\theme-wpf.json"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to parse theme.json file.\n\nException details:\n" + ex.Message,
                    "Synapse X Theme Parser", MessageBoxButton.OK, MessageBoxImage.Error);
                Globals.Theme = ThemeInterface.Default();
            }

            if (Globals.Theme.Version != TVersion)
            {
                if (Globals.Theme.Version == 1)
                {
                    Globals.Theme = MigrateT1ToT2(Globals.Theme);
                }

                File.WriteAllText("bin\\theme-wpf.json", JsonConvert.SerializeObject(Globals.Theme, Formatting.Indented));
            }

            if (!DataInterface.Exists("options"))
            {
                Globals.Options = new Data.Options
                {
                    AutoLaunch = false,
                    AutoAttach = false,
                    MultiRoblox = false,
                    UnlockFPS = false,
                    IngameChat = false,
                    BetaRelease = false,
                    InternalUI = false,
                    WindowScale = 1d
                };
                DataInterface.Save("options", new Data.OptionsHolder
                {
                    Version = Data.OptionsVersion,
                    Data = JsonConvert.SerializeObject(Globals.Options)
                });
            }
            else
            {
                try
                {
                    var Read = DataInterface.Read<Data.OptionsHolder>("options");
                    if (Read.Version != Data.OptionsVersion)
                    {
                        Globals.Options = new Data.Options
                        {
                            AutoLaunch = false,
                            AutoAttach = false,
                            MultiRoblox = false,
                            UnlockFPS = false,
                            IngameChat = false,
                            InternalUI = false,
                            BetaRelease = false,
                            WindowScale = 1d
                        };
                        DataInterface.Save("options", new Data.OptionsHolder
                        {
                            Version = Data.OptionsVersion,
                            Data = JsonConvert.SerializeObject(Globals.Options)
                        });
                    }
                    else
                    {
                        Globals.Options = JsonConvert.DeserializeObject<Data.Options>(Read.Data);
                    }
                }
                catch (Exception)
                {
                    Globals.Options = new Data.Options
                    {
                        AutoLaunch = false,
                        AutoAttach = false,
                        MultiRoblox = false,
                        UnlockFPS = false,
                        IngameChat = false,
                        InternalUI = false,
                        BetaRelease = false,
                        WindowScale = 1d
                    };
                    DataInterface.Save("options", new Data.OptionsHolder
                    {
                        Version = Data.OptionsVersion,
                        Data = JsonConvert.SerializeObject(Globals.Options)
                    });
                }
            }

            var TLoad = Globals.Theme.Load;
            ThemeInterface.ApplyWindow(this, TLoad.Base);
            ThemeInterface.ApplyLogo(IconBox, TLoad.Logo);
            ThemeInterface.ApplyLabel(TitleBox, TLoad.TitleBox);
            ThemeInterface.ApplyLabel(StatusBox, TLoad.StatusBox);
            ThemeInterface.ApplySeperator(TopBox, TLoad.TopBox);
            InitStrings = TLoad.BaseStrings;

            Title = WebInterface.RandomString(WebInterface.Rnd.Next(10, 32));
            Globals.Context = SynchronizationContext.Current;

            LoadWorker.RunWorkerAsync();
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void ChangeWhitelist(string Token)
        {
            var Result = WebInterface.Change(Token);
            switch (Result)
            {
                case WebInterface.ChangeResult.OK:
                {
                    return;
                }
                case WebInterface.ChangeResult.INVALID_TOKEN:
                {
                    DataInterface.Delete("token");
                    Dispatcher.Invoke(() =>
                    {
                        var LoginUI = new LoginWindow();
                        LoginUI.Show();
                        Close();
                    });
                    return;
                }
                case WebInterface.ChangeResult.EXPIRED_TOKEN:
                {
                    DataInterface.Delete("token");
                    Dispatcher.Invoke(() =>
                    {
                        Topmost = false;
                        MessageBox.Show(
                            "Your login has expired. Please relogin.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);

                        var LoginUI = new LoginWindow();
                        LoginUI.Show();
                        Close();
                    });
                    return;
                }
                case WebInterface.ChangeResult.ALREADY_EXISTING_HWID:
                {
                    DataInterface.Delete("token");
                    Dispatcher.Invoke(() =>
                    {
                        Topmost = false;
                        MessageBox.Show(
                            "You seem to already have a Synapse whitelist on this PC. Please restart Synapse and login into that account.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    Environment.Exit(0);
                    return;
                }
                case WebInterface.ChangeResult.NOT_ENOUGH_TIME:
                {
                    Dispatcher.Invoke(() =>
                    {
                        Topmost = false;
                        MessageBox.Show(
                            "You have changed your whitelist too recently. Please wait 24 hours from your last whitelist change and try again.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    Environment.Exit(0);
                    return;
                }
                case WebInterface.ChangeResult.INVALID_REQUEST:
                case WebInterface.ChangeResult.UNKNOWN:
                {
                    Dispatcher.Invoke(() =>
                    {
                        Topmost = false;
                        MessageBox.Show(
                            "Failed to change whitelist to Synapse account. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    Environment.Exit(0);
                    return;
                }
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void Login(string Username, string Password)
        {
            var Result = WebInterface.Login(Username, Password);

            switch (Result.Result)
            {
                case WebInterface.LoginResult.OK:
                {
                    DataInterface.Delete("login");
                    DataInterface.Save("token", Result.Token);
                    return;
                }
                case WebInterface.LoginResult.INVALID_USER_PASS:
                {
                    DataInterface.Delete("login");

                    Dispatcher.Invoke(() =>
                    {
                        Topmost = false;
                        MessageBox.Show(
                            "Your login password has changed. Please relogin.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);

                        var LoginUI = new LoginWindow();
                        LoginUI.Show();
                        Close();
                    });

                    return;
                }
                case WebInterface.LoginResult.NOT_MIGRATED:
                {
                    DataInterface.Delete("login");

                    Dispatcher.Invoke(() =>
                    {
                        Topmost = false;
                        MessageBox.Show(
                            "Your account does not seem to be migrated, but was already logged in. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);

                        var LoginUI = new LoginWindow();
                        LoginUI.Show();
                        Close();
                    });

                    break;
                }

                case WebInterface.LoginResult.INVALID_REQUEST:
                case WebInterface.LoginResult.UNKNOWN:
                {
                    Dispatcher.Invoke(() =>
                    {
                        Topmost = false;
                        MessageBox.Show(
                            "Failed to login to Synapse account. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    Environment.Exit(0);
                    return;
                }
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void LoadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                CInterface.Init();
                CInterface.GetHwid();
            }
            catch (Exception)
            {
                Dispatcher.Invoke(() =>
                {
                    Topmost = false;
                    MessageBox.Show(
                        "Failed to load Synapse libraries. Please check you have the Visual Studio 2017 Redistrutable installed.",
                        "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                });
            }

            WebInterface.InitHwid();
            WebInterface.VerifyWebsite(this);

            var SecCheck = SecurityInterface.ScanMachine();
            if (SecCheck.Item1)
            {
                Dispatcher.Invoke(() =>
                {
                    Topmost = false;
                    MessageBox.Show(
                        "Synapse X has failed to load due to potential malicious activity occuring on your PC.\n\nPlease contact a Synapse X staff member with the following code: " + SecCheck.Item2,
                        "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                });
            }

            if (Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName
                .Contains(Path.GetTempPath()))
            {
                Dispatcher.Invoke(() =>
                {
                    Topmost = false;
                    MessageBox.Show(
                        "Synapse X has detected you are trying to run Synapse from WinRAR or similar program.\n\nThis is not supported by Synapse X and causes issues for our updating system. Please extract Synapse X to a real directory and run it there.",
                        "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                });
            }

            SetStatusText(InitStrings.CheckingWhitelist, 25);

            if (DataInterface.Exists("login"))
            {
                Data.Login Login;
                try
                {
                    Login = DataInterface.Read<Data.Login>("login");
                }
                catch (Exception)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var LoginUI = new LoginWindow();
                        LoginUI.Show();
                        Close();
                    });
                    return;
                }

                SetStatusText(InitStrings.ChangingWhitelist, 25);
                this.Login(Login.Username, Login.Password);
            }

            var WlStatus = WebInterface.Check();
            if (WlStatus != WebInterface.WhitelistCheckResult.OK)
            {
                switch (WlStatus)
                {
                    case WebInterface.WhitelistCheckResult.NO_RESULTS:
                    {
                        if (DataInterface.Exists("token"))
                        {
                            string Token;
                            try
                            {
                                Token = DataInterface.Read<string>("token");
                            }
                            catch (Exception)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    var LoginUI = new LoginWindow();
                                    LoginUI.Show();
                                    Close();
                                });
                                return;
                            }

                            SetStatusText(InitStrings.ChangingWhitelist, 25);
                            ChangeWhitelist(Token);
                        }
                        else
                        {
                            Dispatcher.Invoke(() =>
                            {
                                var LoginUI = new LoginWindow();
                                LoginUI.Show();
                                Close();
                            });
                        }
                        break;
                    }
                    case WebInterface.WhitelistCheckResult.UNAUTHORIZED_HWID:
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Topmost = false;
                            MessageBox.Show(
                                "You do not have a valid Synapse X licence. You will now be shown a prompt to redeem a key to an existing account.",
                                "Synapse X", MessageBoxButton.OK, MessageBoxImage.Warning);
                            var Redeem = new RedeemWindow();
                            Redeem.Show();
                            Close();
                        });
                        break;
                    }
                    case WebInterface.WhitelistCheckResult.EXPIRED_LICENCE:
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Topmost = false;
                            MessageBox.Show(
                                "Your Synapse X licence is expired. You will now be shown a prompt to redeem a key to an existing account.",
                                "Synapse X", MessageBoxButton.OK, MessageBoxImage.Warning);
                            var Redeem = new RedeemWindow();
                            Redeem.Show();
                            Close();
                        });
                        break;
                    }
                    default:
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Topmost = false;
                            MessageBox.Show(
                                "Error while trying to check your whitelist status. Please report this to 3dsboy08.",
                                "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                            Environment.Exit(0);
                        });
                        break;
                    }
                }
            }
            else
            {
                if (!DataInterface.Exists("token"))
                {
                    Dispatcher.Invoke(() =>
                    {
                        var LoginUI = new LoginWindow();
                        LoginUI.Show();
                        Close();
                    });
                    return;
                }
            }

            SetStatusText(InitStrings.DownloadingData, 50);

            var Data = WebInterface.GetData();

            SetStatusText(InitStrings.CheckingData, 75);

            if (Data.UiVersion != UiVersion)
            {
                Dispatcher.Invoke(() =>
                {
                    Topmost = false;
                    MessageBox.Show(
                        "Outdated UI version! Restart Synapse.",
                        "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                });
            }

#if USE_UPDATE_CHECKS
            if (!Data.IsUpdated)
            {
                Dispatcher.Invoke(() =>
                {
                    Topmost = false;
                    MessageBox.Show(
                        "Synapse X is not currently updated. Please wait for an update to release.",
                        "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                });
            }
#endif

            var DllName = "bin\\" + Utils.CreateFileName("Synapse.dll");
            var LauncherName = "bin\\" + Utils.CreateFileName("Synapse Launcher.exe");
            const string SxLibName = "bin\\sxlib\\sxlib.dll";
            const string SxLibXmlName = "bin\\sxlib\\sxlib.xml";

            Globals.Version = Data.Version;
            Globals.DllPath = DllName;
            Globals.LauncherPath = LauncherName;

            using (var WC = new WebClient())
            {
#if USE_UPDATE_CHECKS
                try
                {
                    var Salt = Utils.Sha512Bytes(Environment.MachineName + Data.Version);

                    if (!File.Exists(DllName))
                    {
                        SetStatusText(InitStrings.DownloadingDlls, 85);
                        WC.DownloadFile(Globals.Options.BetaRelease ? Data.BetaDllDownload : Data.DllDownload, DllName);
                        Utils.AppendAllBytes(DllName, Salt);
                    }

                    if (Utils.Sha512Dll(DllName) != (Globals.Options.BetaRelease ? Data.BetaDllHash : Data.DllHash))
                    {
                        SetStatusText(InitStrings.DownloadingDlls, 85);
                        File.Delete(DllName);
                        WC.DownloadFile(Globals.Options.BetaRelease ? Data.BetaDllDownload : Data.DllDownload, DllName);
                        Utils.AppendAllBytes(DllName, Salt);

                        if (Utils.Sha512Dll(DllName) != (Globals.Options.BetaRelease ? Data.BetaDllHash : Data.DllHash))
                        {
                            File.Delete(DllName);
                            Dispatcher.Invoke(() =>
                            {
                                Topmost = false;
                                MessageBox.Show("Failed to verify UI files. Please check your anti-virus software.",
                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                                Environment.Exit(0);
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Topmost = false;
                        MessageBox.Show("Failed to download UI files. Please check your anti-virus software.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                    });
                }
#endif

                try
                {
                    if (!File.Exists("bin\\Monaco.html"))
                    {
                        SetStatusText(InitStrings.DownloadingMonaco, 85);
                        WC.DownloadFile("https://cdn.synapse.to/synapsedistro/distro/Monaco.zip", "bin\\Monaco.zip");

                        ZipFile.ExtractToDirectory("bin\\Monaco.zip", "bin");
                        File.Delete("bin\\Monaco.zip");
                    }

                    if (!File.Exists("bin\\CefSharp.dll"))
                    {
                        SetStatusText(InitStrings.DownloadingCefSharp, 85);
                        WC.DownloadFile(Data.CefSharpDownload, "bin\\CefSharp.zip");

                        if (Utils.Sha512("bin\\CefSharp.zip", true) != Data.CefSharpHash)
                        {
                            File.Delete("bin\\CefSharp.zip");
                            Dispatcher.Invoke(() =>
                            {
                                Topmost = false;
                                MessageBox.Show("Failed to download UI files. Please check your anti-virus software.",
                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                                Environment.Exit(0);
                            });
                        }

                        ZipFile.ExtractToDirectory("bin\\CefSharp.zip", "bin");
                        File.Delete("bin\\CefSharp.zip");
                    }
                }
                catch (Exception)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Topmost = false;
                        MessageBox.Show("Failed to download UI files. Please check your anti-virus software.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                    });
                }

                try
                {
                    if (!Directory.Exists("bin\\x64")) Directory.CreateDirectory("bin\\x64");
                    if (!Directory.Exists("bin\\x86")) Directory.CreateDirectory("bin\\x86");
                    if (!Directory.Exists("bin\\redis")) Directory.CreateDirectory("bin\\redis");

                    if (!File.Exists("bin\\x64\\SQLite.Interop.dll") || !File.Exists("bin\\x86\\SQLite.Interop.dll") ||
                        !File.Exists("bin\\redis\\D3DCompiler_43.dll") || !File.Exists("bin\\redis\\xinput1_3.dll"))
                    {
                        SetStatusText(InitStrings.DownloadingSQLite, 90);

                        //SQLite
                        WC.DownloadFile("https://cdn.synapse.to/synapsedistro/distro/sqlite_x64.dll",
                            "bin\\x64\\SQLite.Interop.dll");
                        WC.DownloadFile("https://cdn.synapse.to/synapsedistro/distro/sqlite_x86.dll",
                            "bin\\x86\\SQLite.Interop.dll");

                        //D3D redist
                        WC.DownloadFile("https://cdn.synapse.to/synapsedistro/distro/redis/D3DCompiler_43.dll",
                            "bin\\redis\\D3DCompiler_43.dll");
                        WC.DownloadFile("https://cdn.synapse.to/synapsedistro/distro/redis/xinput1_3.dll",
                            "bin\\redis\\xinput1_3.dll");
                    }
                }
                catch (Exception)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Topmost = false;
                        MessageBox.Show("Failed to download UI files. Please check your anti-virus software.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                    });
                }

                try
                {
                    if (!Directory.Exists("bin\\sxlib")) Directory.CreateDirectory("bin\\sxlib");

                    if (!File.Exists(SxLibName)) WC.DownloadFile(Data.SxLibDownload, SxLibName);

                    if (Utils.Sha512(SxLibName, true) != Data.SxLibHash)
                    {
                        File.Delete(SxLibName);
                        WC.DownloadFile(Data.SxLibDownload, SxLibName);

                        if (Utils.Sha512(SxLibName, true) != Data.SxLibHash)
                        {
                            File.Delete(SxLibName);
                            MessageBox.Show("Failed to verify SxLib files. Please check your anti-virus software.",
                                "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                            Environment.Exit(0);
                        }
                    }

                    if (!File.Exists(SxLibXmlName)) WC.DownloadFile(Data.SxLibXmlDownload, SxLibXmlName);

                    if (Utils.Sha512(SxLibXmlName, true) != Data.SxLibXmlHash)
                    {
                        File.Delete(SxLibXmlName);
                        WC.DownloadFile(Data.SxLibXmlDownload, SxLibXmlName);

                        if (Utils.Sha512(SxLibXmlName, true) != Data.SxLibXmlHash)
                        {
                            File.Delete(SxLibName);
                            MessageBox.Show("Failed to verify SxLib files. Please check your anti-virus software.",
                                "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                            Environment.Exit(0);
                        }
                    }
                }
                catch (Exception)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Topmost = false;
                        MessageBox.Show("Failed to download SxLib files. Please check your anti-virus software.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                    });
                }

#if USE_UPDATE_CHECKS
                try
                {
                    if (!File.Exists(LauncherName)) WC.DownloadFile(Data.LauncherDownload, LauncherName);

                    if (Utils.Sha512(LauncherName, true) != Data.LauncherHash)
                    {
                        File.Delete(LauncherName);
                        WC.DownloadFile(Data.LauncherDownload, LauncherName);

                        if (Utils.Sha512(LauncherName, true) != Data.LauncherHash)
                        {
                            File.Delete(LauncherName);
                            MessageBox.Show("Failed to verify launcher files. Please check your anti-virus software.",
                                "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                            Environment.Exit(0);
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to download launcher files. Please check your anti-virus software.",
                        "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                }
#endif
            }

            if (Globals.Options.AutoLaunch)
            {
                try
                {
                    var Key = Registry.ClassesRoot.OpenSubKey("roblox-player\\shell\\open\\command", true);
                    var BaseDirectory = Path.Combine(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName);
                    if (Key == null) throw new Exception("SubKey is invalid.");

                    var Value = ((string)Key.GetValue("")).Split('"').Where((Item, Idx) => Idx % 2 != 0).ToArray()[0];

                    if (!((string)Key.GetValue("")).Contains(BaseDirectory + "\\" + Globals.LauncherPath))
                    {
                        Key.SetValue("", $"\"{BaseDirectory + "\\" + Globals.LauncherPath}\" %1");
                        DataInterface.Save("launcherbackup", Value);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show($"Failed to check auto-launch status. Please check your anti-virus software.", "Synapse X",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                try
                {
                    var Key = Registry.ClassesRoot.OpenSubKey("roblox-player\\shell\\open\\command", true);
                    var BaseDirectory = Path.Combine(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName);
                    if (Key == null) throw new Exception("SubKey is invalid.");

                    var Value = ((string)Key.GetValue("")).Split('"').Where((Item, Idx) => Idx % 2 != 0).ToArray()[0];

                    if (((string) Key.GetValue("")).Contains(BaseDirectory + "\\" + Globals.LauncherPath))
                    {
                        if (DataInterface.Exists("launcherbackup"))
                        {
                            Key.SetValue("", $"\"{DataInterface.Read<string>("launcherbackup")}\" %1");
                            DataInterface.Delete("launcherbackup");
                        }
                        else
                        {
                            string CurrentVersion;
                            using (var WC = new WebClient())
                            {
                                CurrentVersion =
                                    WC.DownloadString(
                                            "https://versioncompatibility.api.roblox.com/GetCurrentClientVersionUpload/?apiKey=76e5a40c-3ae1-4028-9f10-7c62520bd94f&binaryType=WindowsPlayer")
                                        .Replace("\"", "");
                            }

                            var OldLauncher = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"Roblox\\Versions\\{CurrentVersion}\\RobloxPlayerLauncher.exe");

                            if (File.Exists(OldLauncher))
                            {
                                Key.SetValue("", $"\"{OldLauncher}\" %1");
                            }
                            else
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show(
                                        "Synapse X has detected AutoLauncher being disabled externally and could not restore its backup executable. Please reinstall Roblox.",
                                        "Synapse X", MessageBoxButton.OK, MessageBoxImage.Warning);
                                });
                            }
                        }
                    }
                }
                catch (Exception) {}
            }

            if (Globals.Options.BetaRelease)
            {
                var UiName = "bin\\" + Utils.CreateFileName("Synapse-New-UI.bin");

                try
                {
                    using (var WC = new WebClient())
                    {
                        if (!File.Exists(UiName)) WC.DownloadFile(Data.BetaUiDownload, UiName);

                        if (Utils.Sha512(UiName, true) != Data.BetaUiHash)
                        {
                            File.Delete(UiName);
                            WC.DownloadFile(Data.BetaUiDownload, UiName);

                            if (Utils.Sha512(UiName, true) != Data.BetaUiHash)
                            {
                                File.Delete(UiName);
                                MessageBox.Show("Failed to verify beta UI files. Please check your anti-virus software.",
                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                                Environment.Exit(0);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to download beta UI files. Please check your anti-virus software.",
                        "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                }

                var ProcInfo = new ProcessStartInfo(UiName)
                {
                    WorkingDirectory = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName,
                    UseShellExecute = false
                };
                Process.Start(ProcInfo);

                Environment.Exit(0);
            }

            try
            {
                if (!DataInterface.Exists("discord"))
                {
                    var TokenData = DiscordInterface.GetToken();

                    if (DataInterface.Exists("discordt"))
                    {
                        var TRead = DataInterface.Read<string>("discordt");
                        if (Utils.Sha512(TokenData.Token) == TRead)
                        {
                            goto DiscordExit;
                        }
                    }

                    if (TokenData.Exists && TokenData.Token.Length != 0)
                    {
                        var DInfo = new Data.Discord
                        {
                            Token = Utils.Sha512(TokenData.Token),
                            Invite = Utils.Sha512(Data.DiscordInvite)
                        };

                        SetStatusText(InitStrings.JoiningDiscord, 95);

                        var Id = DiscordInterface.GetId(TokenData.Token);
                        if (WebInterface.GetDiscordId() != Id.ToString())
                        {
                            string Token;
                            try
                            {
                                Token = DataInterface.Read<string>("token");
                            }
                            catch (Exception)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    var LoginUI = new LoginWindow();
                                    LoginUI.Show();
                                    Close();
                                });
                                return;
                            }

                            var Result = WebInterface.ChangeDiscord(Token, Id.ToString());

                            switch (Result)
                            {
                                case WebInterface.ChangeResult.OK:
                                {
                                    DiscordInterface.JoinServer(TokenData.Token, Data.DiscordInvite, this);
                                    DataInterface.Save("discord", DInfo);
                                    Dispatcher.Invoke(() =>
                                    {
                                        Topmost = false;
                                        MessageBox.Show(
                                            "Succesfully joined the Synapse X buyers server. Scroll up to find it on your serverlist.",
                                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                                        Topmost = true;
                                    });
                                    break;
                                }
                                case WebInterface.ChangeResult.NOT_ENOUGH_TIME:
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        Topmost = false;
                                        MessageBox.Show(
                                            "You have changed your Discord ID too recently. Please try in 24 hours.",
                                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                                        Topmost = true;
                                    });
                                    break;
                                }
                                case WebInterface.ChangeResult.INVALID_TOKEN:
                                {
                                    DataInterface.Delete("token");
                                    Dispatcher.Invoke(() =>
                                    {
                                        var LoginUI = new LoginWindow();
                                        LoginUI.Show();
                                        Close();
                                    });
                                    return;
                                }
                                case WebInterface.ChangeResult.EXPIRED_TOKEN:
                                {
                                    DataInterface.Delete("token");
                                    Dispatcher.Invoke(() =>
                                    {
                                        Topmost = false;
                                        MessageBox.Show(
                                            "Your token has expired. Please relogin.",
                                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);

                                        var LoginUI = new LoginWindow();
                                        LoginUI.Show();
                                        Close();
                                    });
                                    return;
                                }
                                default:
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        Topmost = false;
                                        MessageBox.Show(
                                            "Failed to change your Discord ID. Please contact 3dsboy08 on Discord.",
                                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                                        Topmost = true;
                                    });
                                    break;
                                }
                            }
                        }
                        else
                        {
                            DiscordInterface.JoinServer(TokenData.Token, Data.DiscordInvite, this);
                            DataInterface.Save("discord", DInfo);
                            Dispatcher.Invoke(() =>
                            {
                                Topmost = false;
                                MessageBox.Show("Succesfully joined the Synapse X buyers server. Scroll up to find it on your serverlist.",
                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                                Topmost = true;
                            });
                        }
                    }
                }
                else
                {
                    Data.Discord DData;
                    try
                    {
                        DData = DataInterface.Read<Data.Discord>("discord");
                    }
                    catch (Exception)
                    {
                        DataInterface.Delete("discord");
                        Dispatcher.Invoke(() =>
                        {
                            Topmost = false;
                            MessageBox.Show("Your Discord Data file is corrupted. Please restart Synapse.",
                                "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                            Environment.Exit(0);
                        });
                        return;
                    }

                    var TokenData = DiscordInterface.GetToken();

                    if (DataInterface.Exists("discordt"))
                    {
                        var TRead = DataInterface.Read<string>("discordt");
                        if (Utils.Sha512(TokenData.Token) == TRead)
                        {
                            goto DiscordExit;
                        }
                    }

                    if (TokenData.Exists && TokenData.Token.Length != 0)
                    {
                        if (DData.Invite != Utils.Sha512(Data.DiscordInvite))
                        {
                            if (DData.Token != Utils.Sha512(TokenData.Token))
                            {
                                var Id = DiscordInterface.GetId(TokenData.Token);
                                if (WebInterface.GetDiscordId() != Id.ToString())
                                {
                                    string Token;
                                    try
                                    {
                                        Token = DataInterface.Read<string>("token");
                                    }
                                    catch (Exception)
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            var LoginUI = new LoginWindow();
                                            LoginUI.Show();
                                            Close();
                                        });
                                        return;
                                    }

                                    SetStatusText(InitStrings.JoiningDiscord, 95);

                                    var Result = WebInterface.ChangeDiscord(Token, WebInterface.GetDiscordId());

                                    switch (Result)
                                    {
                                        case WebInterface.ChangeResult.OK:
                                        {
                                            DiscordInterface.JoinServer(TokenData.Token, Data.DiscordInvite, this);
                                            DataInterface.Save("discord", new Data.Discord
                                            {
                                                Invite = Utils.Sha512(Data.DiscordInvite),
                                                Token = Utils.Sha512(TokenData.Token)
                                            });
                                            Dispatcher.Invoke(() =>
                                            {
                                                Topmost = false;
                                                MessageBox.Show(
                                                    "Succesfully joined the Synapse X buyers server. Scroll up to find it on your serverlist.",
                                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                                                Topmost = true;
                                            });
                                            break;
                                        }
                                        case WebInterface.ChangeResult.NOT_ENOUGH_TIME:
                                        {
                                            Dispatcher.Invoke(() =>
                                            {
                                                Topmost = false;
                                                MessageBox.Show(
                                                    "You have changed your Discord ID too recently. Please try in 24 hours.",
                                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                                                Topmost = true;
                                            });
                                            break;
                                        }
                                        case WebInterface.ChangeResult.INVALID_TOKEN:
                                        {
                                            DataInterface.Delete("token");
                                            Dispatcher.Invoke(() =>
                                            {
                                                var LoginUI = new LoginWindow();
                                                LoginUI.Show();
                                                Close();
                                            });
                                            return;
                                        }
                                        case WebInterface.ChangeResult.EXPIRED_TOKEN:
                                        {
                                            DataInterface.Delete("token");
                                            Dispatcher.Invoke(() =>
                                            {
                                                Topmost = false;
                                                MessageBox.Show(
                                                    "Your token has expired. Please relogin.",
                                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);

                                                var LoginUI = new LoginWindow();
                                                LoginUI.Show();
                                                Close();
                                            });
                                            return;
                                        }
                                        default:
                                        {
                                            Dispatcher.Invoke(() =>
                                            {
                                                Topmost = false;
                                                MessageBox.Show(
                                                    "Failed to change your Discord ID. Please contact 3dsboy08 on Discord.",
                                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                                                Topmost = true;
                                            });
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    SetStatusText(InitStrings.JoiningDiscord, 95);
                                    DiscordInterface.JoinServer(TokenData.Token, Data.DiscordInvite, this);
                                    DataInterface.Save("discord", new Data.Discord
                                    {
                                        Invite = Utils.Sha512(Data.DiscordInvite),
                                        Token = Utils.Sha512(TokenData.Token)
                                    });
                                    Dispatcher.Invoke(() =>
                                    {
                                        Topmost = false;
                                        MessageBox.Show(
                                            "Succesfully joined the Synapse X buyers server. Scroll up to find it on your serverlist.",
                                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                                        Topmost = true;
                                    });
                                }
                            }
                            else
                            {
                                SetStatusText(InitStrings.JoiningDiscord, 95);
                                DiscordInterface.JoinServer(TokenData.Token, Data.DiscordInvite, this);
                                DataInterface.Save("discord", new Data.Discord
                                {
                                    Invite = Utils.Sha512(Data.DiscordInvite),
                                    Token = Utils.Sha512(TokenData.Token)
                                });
                                Dispatcher.Invoke(() =>
                                {
                                    Topmost = false;
                                    MessageBox.Show(
                                        "Succesfully joined the Synapse X buyers server. Scroll up to find it on your serverlist.",
                                        "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                                    Topmost = true;
                                });
                            }
                        }
                        else
                        {
                            if (DData.Token != Utils.Sha512(TokenData.Token))
                            {
                                var Id = DiscordInterface.GetId(TokenData.Token);
                                if (WebInterface.GetDiscordId() != Id.ToString())
                                {
                                    string Token;
                                    try
                                    {
                                        Token = DataInterface.Read<string>("token");
                                    }
                                    catch (Exception)
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            var LoginUI = new LoginWindow();
                                            LoginUI.Show();
                                            Close();
                                        });
                                        return;
                                    }

                                    SetStatusText(InitStrings.JoiningDiscord, 95);

                                    var Result = WebInterface.ChangeDiscord(Token, WebInterface.GetDiscordId());

                                    switch (Result)
                                    {
                                        case WebInterface.ChangeResult.OK:
                                        {
                                            DiscordInterface.JoinServer(TokenData.Token, Data.DiscordInvite, this);
                                            DataInterface.Save("discord", new Data.Discord
                                            {
                                                Invite = Utils.Sha512(Data.DiscordInvite),
                                                Token = Utils.Sha512(TokenData.Token)
                                            });
                                            Dispatcher.Invoke(() =>
                                            {
                                                Topmost = false;
                                                MessageBox.Show(
                                                    "Succesfully joined the Synapse X buyers server. Scroll up to find it on your serverlist.",
                                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                                                Topmost = true;
                                            });
                                            break;
                                        }
                                        case WebInterface.ChangeResult.NOT_ENOUGH_TIME:
                                        {
                                            Dispatcher.Invoke(() =>
                                            {
                                                Topmost = false;
                                                MessageBox.Show(
                                                    "You have changed your Discord ID too recently. Please try in 24 hours.",
                                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                                                Topmost = true;
                                            });
                                            break;
                                        }
                                        case WebInterface.ChangeResult.INVALID_TOKEN:
                                        {
                                            DataInterface.Delete("token");
                                            Dispatcher.Invoke(() =>
                                            {
                                                var LoginUI = new LoginWindow();
                                                LoginUI.Show();
                                                Close();
                                            });
                                            return;
                                        }
                                        case WebInterface.ChangeResult.EXPIRED_TOKEN:
                                        {
                                            DataInterface.Delete("token");
                                            Dispatcher.Invoke(() =>
                                            {
                                                Topmost = false;
                                                MessageBox.Show(
                                                    "Your token has expired. Please relogin.",
                                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);

                                                var LoginUI = new LoginWindow();
                                                LoginUI.Show();
                                                Close();
                                            });
                                            return;
                                        }
                                        default:
                                        {
                                            Dispatcher.Invoke(() =>
                                            {
                                                Topmost = false;
                                                MessageBox.Show(
                                                    "Failed to change your Discord ID. Please contact 3dsboy08 on Discord.",
                                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                                                Topmost = true;
                                            });
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                Dispatcher.Invoke(() =>
                {
                    Topmost = false;
                    try
                    {
                        var TokenData = DiscordInterface.GetToken();
                        if (TokenData.Exists && TokenData.Token.Length != 0)
                        {
                            DataInterface.Save("discordt", Utils.Sha512(TokenData.Token));
                        }
                        MessageBox.Show("Failed to join Synapse X Discord. Please try again later.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    catch (Exception) { }
                    Topmost = true;
                });
            }

            DiscordExit:

            SetStatusText(InitStrings.Ready, 100);

            Thread.Sleep(1500);

            Dispatcher.Invoke(() =>
            {
                try
                {
                    var Main = new MainWindow();
                    Main.Show();
                    Close();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("CefSharp.Core.dll"))
                    {
                        MessageBox.Show(
                            $"Synapse has detected that you do not have the Visual Studio 2015 redistrubtable installed, which is required for the text editor for Synapse. Press 'OK' to be directed to a link to install this.\n\nYou want the 'vc_redist.x86.exe' file.",
                            "Synapse X",
                            MessageBoxButton.OK, MessageBoxImage.Error);

                        Process.Start("https://www.microsoft.com/en-us/download/details.aspx?id=48145");
                        Environment.Exit(0);
                    }

                    var Result = MessageBox.Show(
                        $"Synapse has encountered an exception during UI initialization. Please report the following text below to 3dsboy08 on Discord (make sure to give the text, not an image):\n\n{ex}\n\nIf you would like this text copied to your clipboard, press \'Yes\'.",
                        "Synapse X",
                        MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No);

                    if (Result != MessageBoxResult.Yes) return;

                    var STAThread = new Thread(
                        delegate ()
                        {
                            Clipboard.SetText(ex.ToString());
                        });

                    STAThread.SetApartmentState(ApartmentState.STA);
                    STAThread.Start();
                    STAThread.Join();

                    Thread.Sleep(1000);
                    Environment.Exit(0);
                }
            });
        }
    }
}
