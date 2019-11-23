#define USE_UPDATE_CHECKS
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using Newtonsoft.Json;
using sxlib.Internal;
using sxlib.Static;
using Process = System.Diagnostics.Process;

namespace sxlib.Specialized
{
    public class SxLibBase
    {
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private bool LoadMutex;

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private readonly string SynapseDir;

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private int RobloxIdOverride;

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private int RobloxIdTemp;

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static int RbxId;

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private bool ScriptHubOpen;

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private bool ScriptHubInit;

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private bool IsInlineUpdating;

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private bool IsLoaded;

        private ProcessWatcher Watcher;
        private readonly BackgroundWorker AttachWorker = new BackgroundWorker();
        private readonly BackgroundWorker HubWorker = new BackgroundWorker();

        /// <summary>
        /// Loading events for Synapse (will be sent as callbacks with the 'Load' function)
        /// </summary>
        public enum SynLoadEvents
        {
            /// <summary>
            /// Unknown error. You should usually exit your application if this is received.
            /// </summary>
            UNKNOWN,

            /// <summary>
            /// The user is not logged in to Synapse X. SxLib does not include methods to log a user in at the present moment, you must tell the user to login via the official UI.
            /// </summary>
            NOT_LOGGED_IN,

            /// <summary>
            /// Synapse X is currently not updated.
            /// </summary>
            NOT_UPDATED,

            /// <summary>
            /// Synapse X detected a discrepancy within its data. You should usually exit your application if this is received.
            /// </summary>
            FAILED_TO_VERIFY,

            /// <summary>
            /// Synapse X failed to download needed UI files. This is usually caused by anti-virus software.
            /// </summary>
            FAILED_TO_DOWNLOAD,

            /// <summary>
            /// User is not whitelisted for Synapse X.
            /// </summary>
            UNAUTHORIZED_HWID,

            /// <summary>
            /// This is a rare error - tell the user to use the official UI to fix this issue.
            /// </summary>
            ALREADY_EXISTING_WL,

            /// <summary>
            /// There was not enough time preceding the last whitelist change. Whitelist changes can only happen every 24 hours.
            /// </summary>
            NOT_ENOUGH_TIME,

            /// <summary>
            /// The 'Checking Whitelist' phase of Synapse X initialization.
            /// </summary>
            CHECKING_WL,

            /// <summary>
            /// The 'Changing Whitelist phase of Synapse X initialization.
            ///
            /// This won't occur as often, only if the user requires a whitelist change. You might get an error code after this.
            /// </summary>
            CHANGING_WL,

            /// <summary>
            /// The 'Downloading Data' phase of Synapse X initialization.
            /// </summary>
            DOWNLOADING_DATA,

            /// <summary>
            /// The 'Checking Data' phase of Synapse X initialization.
            /// </summary>
            CHECKING_DATA,

            /// <summary>
            /// The 'Downloading DLLs' phase of Synapse X initialization.
            /// </summary>
            DOWNLOADING_DLLS,

            /// <summary>
            /// Synapse X has loaded successfully and can now be used.
            /// </summary>
            READY,
        }

        /// <summary>
        /// Attaching events for Synapse (will be sent as callbacks with the 'Attach' function)
        /// </summary>
        public enum SynAttachEvents
        {
            /// <summary>
            /// The 'Checking' phase of Synapse X attaching.
            ///
            /// Note that this event may be sent if the user has Auto-Attach or Auto-Launch enabled, even if you did not invoke the SxLib.Attach() method.
            /// </summary>
            CHECKING,

            /// <summary>
            /// The 'Injecting' phase of Synapse X attaching.
            /// </summary>
            INJECTING,

            /// <summary>
            /// The 'Checking Whitelist' phase of Synapse X attaching.
            /// </summary>
            CHECKING_WHITELIST,

            /// <summary>
            /// The 'Scanning' phase of Synapse X attaching.
            ///
            /// Note this event is also sent in teleports as well.
            /// </summary>
            SCANNING,

            /// <summary>
            /// Synapse X has successfully attached.
            /// </summary>
            READY,

            /// <summary>
            /// Synapse X has failed to attach - generic error.
            /// </summary>
            FAILED_TO_ATTACH,

            /// <summary>
            /// Synapse X has failed to find Roblox.
            /// </summary>
            FAILED_TO_FIND,

            /// <summary>
            /// Synapse X has detected they are not running the latest version of Synapse X. Synapse will now update itself.
            /// </summary>
            NOT_RUNNING_LATEST_VER_UPDATING,

            /// <summary>
            /// This is if Synapse X is updating itself and is currently updating its DLLs.
            /// </summary>
            UPDATING_DLLS,

            /// <summary>
            /// This is if Synapse X has detected an update, is not released yet.
            /// </summary>
            NOT_UPDATED,
            
            /// <summary>
            /// This is if Synapse X auto-update failed.
            /// </summary>
            FAILED_TO_UPDATE,           
            
            /// <summary>
            /// This event is called after Synapse X updates, and also specifies 'Param' as a string (the new version string)
            /// </summary>
            REINJECTING,

            /// <summary>
            /// This event is sent if you attempt to use the SxLib.Execute() method without being attached.
            /// </summary>
            NOT_INJECTED,

            /// <summary>
            /// This event is sent if you attempt to use the SxLib.Attach() method while already being attached.
            /// </summary>
            ALREADY_INJECTED,

            /// <summary>
            /// This event is sent if Synapse X detects a new Roblox process being created.
            /// </summary>
            PROC_CREATION,

            /// <summary>
            /// This event is sent if Synapse X detects a Roblox process being deleted.
            /// </summary>
            PROC_DELETION,
        }

        /// <summary>
        /// A entry in the Synapse X script hub.
        /// </summary>
        public class SynHubEntry
        {
            [Obfuscation(Feature = "virtualization", Exclude = false)]
            private readonly string Url;
            private readonly SxLibBase Parent;

            /// <summary>
            /// A name for the script.
            /// </summary>
            public string Name;

            /// <summary>
            /// A description of the script.
            /// </summary>
            public string Description;

            /// <summary>
            /// A URL to a picture of the script in use.
            /// </summary>
            public string Picture;

            /// <summary>
            /// Do not use this constructor.
            /// </summary>
            /// <param name="_Url"></param>
            /// <param name="_Parent"></param>
            public SynHubEntry(string _Url, SxLibBase _Parent)
            {
                Url = _Url;
                Parent = _Parent;
            }

            /// <summary>
            /// Execute the script.
            /// </summary>
            [Obfuscation(Feature = "virtualization", Exclude = false)]
            public void Execute()
            {
                using (var WC = new WebClient())
                {
                    WC.Proxy = null;

                    Parent.ExecuteInternal(WC.DownloadString(Url));
                }
            }
        }

        protected delegate void SynLoadDelegate(SynLoadEvents Event, object Param = null);
        protected delegate void SynAttachDelegate(SynAttachEvents Event, object Param = null);
        protected delegate void SynScriptHubDelegate(List<SynHubEntry> Entries);

        private delegate void InteractMessageEventHandler(object sender, string Input);

        protected virtual void VerifyWebsite() { }

        protected virtual string VerifyWebsiteWithVersion()
        {
            return "";
        }

        protected event SynLoadDelegate LoadEventInternal;
        protected event SynAttachDelegate AttachEventInternal;
        protected event SynScriptHubDelegate HubEventInternal;

        private event InteractMessageEventHandler InteractMessageRecieved;

        //Only allow us to instantiate.
        protected SxLibBase(string _SynapseDir)
        {
            SynapseDir = _SynapseDir;

            if (!SynapseDir.EndsWith("\\")) SynapseDir += "\\";
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        protected bool LoadInternal()
        {
            if (IsLoaded) throw new InvalidOperationException("SxLib is already loaded.");

            DataInterface.BaseDir = SynapseDir;

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

            if (LoadMutex) return false;
            LoadMutex = true;

            new Thread(LoadThread).Start();

            return true;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        protected bool AttachInternal()
        {
            if (!IsLoaded) throw new InvalidOperationException("You need to load SxLib before you can attach it. Please call SxLib.Load() and wait for the 'READY' event before attaching.");
            if (AttachWorker.IsBusy || IsInlineUpdating) return false;

            AttachWorker.RunWorkerAsync();
            return true;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        protected bool ScriptHubInternal()
        {
            if (!IsLoaded) throw new InvalidOperationException("You need to load SxLib before you can attach it. Please call SxLib.Load() and wait for the 'READY' event before attaching.");
            if (ScriptHubOpen) return false;
            if (ScriptHubInit) return false;

            ScriptHubOpen = true;
            ScriptHubInit = true;

            HubWorker.RunWorkerAsync();

            return true;
        }

        protected void ScriptHubMarkAsClosedInternal()
        {
            if (!IsLoaded) throw new InvalidOperationException("You need to load SxLib before you can attach it. Please call SxLib.Load() and wait for the 'READY' event before attaching.");
            ScriptHubOpen = false;
        }

        protected Data.Options GetOptionsInternal()
        {
            if (!IsLoaded) throw new InvalidOperationException("You need to load SxLib before you can attach it. Please call SxLib.Load() and wait for the 'READY' event before attaching.");
            return Globals.Options;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        protected void SetOptionsInternal(Data.Options Options)
        {
            if (!IsLoaded) throw new InvalidOperationException("You need to load SxLib before you can attach it. Please call SxLib.Load() and wait for the 'READY' event before attaching.");
            var OldOptions = Globals.Options;

            Globals.Options = Options;

            DataInterface.Save("options", new Data.OptionsHolder
            {
                Version = Data.OptionsVersion,
                Data = JsonConvert.SerializeObject(Globals.Options)
            });

            if (OldOptions.BetaRelease != Options.BetaRelease)
            {
                MessageBox.Show(
                    "You have chosen to either enable/disable Synapse X beta releases. You must now restart Synapse X in order to install the beta release.",
                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(0);
            }

            if (OldOptions.AutoLaunch != Options.AutoLaunch)
            {
                try
                {
                    var Key = Registry.ClassesRoot.OpenSubKey("roblox-player\\shell\\open\\command", true);
                    if (Key == null) throw new Exception("SubKey is invalid.");

                    if (Options.AutoLaunch)
                    {
                        var Value = ((string) Key.GetValue("")).Split('"').Where((Item, Idx) => Idx % 2 != 0).ToArray()[0];
                        Key.SetValue("", $"\"{Globals.LauncherPath}\" %1");
                        DataInterface.Save("launcherbackup", Value);
                    }
                    else
                    {
                        if (!DataInterface.Exists("launcherbackup"))
                        {
                            MessageBox.Show("Failed to get launcher backup. You should reinstall Roblox.", "Synapse X",
                                MessageBoxButton.OK, MessageBoxImage.Warning);

                            return;
                        }

                        Key.SetValue("", $"\"{DataInterface.Read<string>("launcherbackup")}\" %1");
                        DataInterface.Delete("launcherbackup");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show($"Failed to {(Options.AutoLaunch ? "setup" : "remove")} AutoLaunch. Please check your anti-virus software.", "Synapse X",
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }
            }

            if (ReadyInternal())
            {
                MessageBox.Show("Some options may not apply until you reinject Synapse.", "Synapse X",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private bool ChangeWhitelist(string Token)
        {
            var Result = WebInterface.Change(Token);

            switch (Result)
            {
                case WebInterface.ChangeResult.OK:
                {
                    return true;
                }

                case WebInterface.ChangeResult.INVALID_TOKEN:
                {
                    DataInterface.Delete("token");
                    LoadEventInternal(SynLoadEvents.NOT_LOGGED_IN);
                    return false;
                }

                case WebInterface.ChangeResult.EXPIRED_TOKEN:
                {
                    DataInterface.Delete("token");
                    LoadEventInternal(SynLoadEvents.NOT_LOGGED_IN);
                    return false;
                }

                case WebInterface.ChangeResult.ALREADY_EXISTING_HWID:
                {
                    DataInterface.Delete("token");
                    LoadEventInternal(SynLoadEvents.ALREADY_EXISTING_WL);
                    return false;
                }

                case WebInterface.ChangeResult.NOT_ENOUGH_TIME:
                {
                    LoadEventInternal(SynLoadEvents.NOT_ENOUGH_TIME);
                    return false;
                }

                case WebInterface.ChangeResult.INVALID_REQUEST:
                case WebInterface.ChangeResult.UNKNOWN:
                {
                    LoadEventInternal(SynLoadEvents.UNKNOWN);
                    return false;
                }
            }

            return false;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private bool Login(string Username, string Password)
        {
            var Result = WebInterface.Login(Username, Password);

            switch (Result.Result)
            {
                case WebInterface.LoginResult.OK:
                {
                    DataInterface.Delete("login");
                    DataInterface.Save("token", Result.Token);
                    return true;
                }

                case WebInterface.LoginResult.INVALID_USER_PASS:
                {
                    DataInterface.Delete("login");

                    LoadEventInternal(SynLoadEvents.NOT_LOGGED_IN);
                    return false;
                }
                case WebInterface.LoginResult.NOT_MIGRATED:
                {
                    DataInterface.Delete("login");

                    LoadEventInternal(SynLoadEvents.NOT_LOGGED_IN);
                    return false;
                }

                case WebInterface.LoginResult.INVALID_REQUEST:
                case WebInterface.LoginResult.UNKNOWN:
                {
                    LoadEventInternal(SynLoadEvents.UNKNOWN);
                    return false;
                }
            }

            return false;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void LoadThread()
        {
            try
            {
                CInterface.Init(SynapseDir);
                CInterface.GetHwid();
            }
            catch (Exception)
            {
                LoadEventInternal(SynLoadEvents.UNKNOWN);
                return;
            }

            WebInterface.InitHwid();
            VerifyWebsite();

            LoadEventInternal(SynLoadEvents.CHECKING_WL);

            if (DataInterface.Exists("login"))
            {
                Data.Login Login;
                try
                {
                    Login = DataInterface.Read<Data.Login>("login");
                }
                catch (Exception)
                {
                    LoadEventInternal(SynLoadEvents.NOT_LOGGED_IN);
                    return;
                }

                LoadEventInternal(SynLoadEvents.CHANGING_WL);

                if (!this.Login(Login.Username, Login.Password)) return;
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
                                LoadEventInternal(SynLoadEvents.NOT_LOGGED_IN);
                                return;
                            }

                            LoadEventInternal(SynLoadEvents.CHANGING_WL);
                            if (!ChangeWhitelist(Token)) return;
                        }
                        else
                        {
                            LoadEventInternal(SynLoadEvents.NOT_LOGGED_IN);
                            return;
                        }

                        break;
                    }

                    case WebInterface.WhitelistCheckResult.UNAUTHORIZED_HWID:
                    {
                        LoadEventInternal(SynLoadEvents.UNAUTHORIZED_HWID);
                        return;
                    }

                    case WebInterface.WhitelistCheckResult.EXPIRED_LICENCE:
                    {
                        LoadEventInternal(SynLoadEvents.UNAUTHORIZED_HWID);
                        return;
                    }

                    default:
                    {
                        LoadEventInternal(SynLoadEvents.UNKNOWN);
                        return;
                    }
                }
            }
            else
            {
                if (!DataInterface.Exists("token"))
                {
                    LoadEventInternal(SynLoadEvents.NOT_LOGGED_IN);
                    return;
                }
            }

            LoadEventInternal(SynLoadEvents.DOWNLOADING_DATA);

            var Data = WebInterface.GetData();

            LoadEventInternal(SynLoadEvents.CHECKING_DATA);

#if USE_UPDATE_CHECKS
            if (!Data.IsUpdated)
            {
                LoadEventInternal(SynLoadEvents.NOT_UPDATED);
                return;
            }
#endif

            var DllName = SynapseDir + "bin\\" + Utils.CreateFileName("Synapse.dll");
            var LauncherName = SynapseDir + "bin\\" + Utils.CreateFileName("Synapse Launcher.exe");

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
                        LoadEventInternal(SynLoadEvents.DOWNLOADING_DLLS);
                        WC.DownloadFile(Globals.Options.BetaRelease ? Data.BetaDllDownload : Data.DllDownload, DllName);
                        Utils.AppendAllBytes(DllName, Salt);
                    }

                    if (Utils.Sha512Dll(DllName) != (Globals.Options.BetaRelease ? Data.BetaDllHash : Data.DllHash))
                    {
                        LoadEventInternal(SynLoadEvents.DOWNLOADING_DLLS);
                        File.Delete(DllName);
                        WC.DownloadFile(Globals.Options.BetaRelease ? Data.BetaDllDownload : Data.DllDownload, DllName);
                        Utils.AppendAllBytes(DllName, Salt);

                        if (Utils.Sha512Dll(DllName) != (Globals.Options.BetaRelease ? Data.BetaDllHash : Data.DllHash))
                        {
                            File.Delete(DllName);

                            LoadEventInternal(SynLoadEvents.FAILED_TO_VERIFY);
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    LoadEventInternal(SynLoadEvents.FAILED_TO_DOWNLOAD);
                    return;
                }
#endif

                try
                {
                    if (!File.Exists(SynapseDir + "bin\\Monaco.html"))
                    {
                        WC.DownloadFile("https://cdn.synapse.to/synapsedistro/distro/Monaco.zip", SynapseDir + "bin\\Monaco.zip");

                        ZipFile.ExtractToDirectory(SynapseDir + "bin\\Monaco.zip", SynapseDir + "bin");
                        File.Delete(SynapseDir + "bin\\Monaco.zip");
                    }

                    if (!File.Exists(SynapseDir + "bin\\CefSharp.dll"))
                    {
                        WC.DownloadFile(Data.CefSharpDownload, SynapseDir + "bin\\CefSharp.zip");

                        if (Utils.Sha512(SynapseDir + "bin\\CefSharp.zip", true) != Data.CefSharpHash)
                        {
                            File.Delete(SynapseDir + "bin\\CefSharp.zip");

                            LoadEventInternal(SynLoadEvents.FAILED_TO_VERIFY);
                            return;
                        }

                        ZipFile.ExtractToDirectory(SynapseDir + "bin\\CefSharp.zip", SynapseDir + "bin");
                        File.Delete(SynapseDir + "bin\\CefSharp.zip");
                    }
                }
                catch (Exception)
                {
                    LoadEventInternal(SynLoadEvents.FAILED_TO_DOWNLOAD);
                    return;
                }

                try
                {
                    if (!Directory.Exists("bin\\x64")) Directory.CreateDirectory("bin\\x64");
                    if (!Directory.Exists("bin\\x86")) Directory.CreateDirectory("bin\\x86");
                    if (!Directory.Exists("bin\\redis")) Directory.CreateDirectory("bin\\redis");

                    if (!File.Exists("bin\\x64\\SQLite.Interop.dll") || !File.Exists("bin\\x86\\SQLite.Interop.dll") ||
                        !File.Exists("bin\\redis\\D3DCompiler_43.dll") || !File.Exists("bin\\redis\\xinput1_3.dll"))
                    {
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
                    LoadEventInternal(SynLoadEvents.FAILED_TO_DOWNLOAD);
                    return;
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
                    if (Key == null) throw new Exception("SubKey is invalid.");

                    var Value = ((string)Key.GetValue("")).Split('"').Where((Item, Idx) => Idx % 2 != 0).ToArray()[0];

                    if (!((string)Key.GetValue("")).Contains(Globals.LauncherPath))
                    {
                        Key.SetValue("", $"\"{Globals.LauncherPath}\" %1");
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

                    if (((string) Key.GetValue("")).Contains(Globals.LauncherPath))
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
                        }
                    }
                }
                catch (Exception) { }
            }

            StreamReader InteractReader = null;
            StreamReader LaunchReader;

            AttachWorker.DoWork += AttachWorker_DoWork;
            HubWorker.DoWork += HubWorker_DoWork;

            try
            {
                Watcher = new ProcessWatcher("RobloxPlayerBeta.exe");
                Watcher.ProcessCreated += Proc =>
                {
                    AttachEventInternal?.Invoke(SynAttachEvents.PROC_CREATION);

                    if (!Globals.Options.AutoAttach || Globals.Options.AutoLaunch) return;
                    RobloxIdOverride = Convert.ToInt32(Proc.ProcessId);
                    AttachWorker.RunWorkerAsync();
                };
                Watcher.ProcessDeleted += Proc =>
                {
                    if (Proc.ProcessId == RobloxIdTemp)
                    {
                        InteractReader?.Close();
                    }

                    AttachEventInternal?.Invoke(SynAttachEvents.PROC_DELETION);
                };
                Watcher.Start();
            }
            catch (Exception)
            {
                if (!DataInterface.Exists("failnotice"))
                {
                    MessageBox.Show(
                        "Synapse failed to create its process watching agent. AutoLaunch will not be as usable.\n\nUnfortunately, this issue is not fixable without upgrading to Windows 10. Blame Microsoft being terrible.\n\nThis message will not be displayed anymore, sorry for your inconvenience.",
                        "Synapse X", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DataInterface.Save("failnotice", true);
                }
            }

            if (DataInterface.Exists("savedpid"))
            {
                var Saved = DataInterface.Read<Data.SavedPid>("savedpid");

                try
                {
                    if (Process.GetProcessById(Saved.Pid).StartTime == Saved.StartTime)
                    {
                        RbxId = Saved.Pid;
                    }

                    DataInterface.Delete("savedpid");
                }
                catch (Exception)
                {
                    DataInterface.Delete("savedpid");
                }
            }

            new Thread(() =>
            {
                var PS = new PipeSecurity();
                var Rule = new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow);
                PS.AddAccessRule(Rule);
                var Server = new NamedPipeServerStream("SynapseInteract", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, PS);
                Server.WaitForConnection();
                InteractReader = new StreamReader(Server);
                while (true)
                {
                    string Line;
                    try
                    {
                        Line = InteractReader.ReadLine();
                    }
                    catch (Exception)
                    {
                        Line = "SYN_INTERRUPT";
                    }
                    InteractMessageRecieved?.Invoke(this, Line);
                    if (Line != "SYN_READY" && Line != "SYN_REATTACH_READY" && Line != "SYN_INTERRUPT") continue;
                    InteractReader.Close();
                    Server.Close();
                    Thread.Sleep(3000);
                    Server = new NamedPipeServerStream("SynapseInteract", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, PS);
                    Server.WaitForConnection();
                    InteractReader = new StreamReader(Server);
                }
            }).Start();

            new Thread(() =>
            {
                var PS = new PipeSecurity();
                var Rule = new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow);
                PS.AddAccessRule(Rule);
                var Server = new NamedPipeServerStream("SynapseLaunch", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, PS);
                Server.WaitForConnection();
                LaunchReader = new StreamReader(Server);
                while (true)
                {
                    string Line;
                    try
                    {
                        Line = LaunchReader.ReadLine();
                    }
                    catch (Exception)
                    {
                        Line = "SYN_INTERRUPT";
                    }

                    if (Line?.Split('|')[0] == "SYN_LAUNCH_NOTIIFCATION")
                    {
                        RobloxIdTemp = int.Parse(Line.Split('|')[1]);
                        AttachEventInternal?.Invoke(SynAttachEvents.CHECKING);
                    }

                    if (Line?.Split('|')[0] != "SYN_LAUNCH_NOTIIFCATION") continue;
                    LaunchReader.Close();
                    Server.Close();
                    Thread.Sleep(3000);
                    Server = new NamedPipeServerStream("SynapseLaunch", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, PS);
                    Server.WaitForConnection();
                    LaunchReader = new StreamReader(Server);
                }
            }).Start();

            InteractMessageRecieved += delegate (object Sender, string Input)
            {
                switch (Input)
                {
                    case "SYN_CHECK_WL":
                        AttachEventInternal?.Invoke(SynAttachEvents.CHECKING_WHITELIST);
                        break;
                    case "SYN_SCANNING":
                        AttachEventInternal?.Invoke(SynAttachEvents.SCANNING);
                        break;
                    case "SYN_INTERRUPT":
                        AttachEventInternal?.Invoke(SynAttachEvents.FAILED_TO_ATTACH);
                        break;
                }

                if (Input == "SYN_READY")
                {
                    var ProcList = Process.GetProcessesByName("RobloxPlayerBeta");
                    RbxId = RobloxIdTemp == 0 ? ProcList[0].Id : RobloxIdTemp;
                    RobloxIdTemp = 0;
                    var EnableUnlock = Globals.Options.UnlockFPS ? "TRUE" : "FALSE";
                    var EnableInternalUI = Globals.Options.InternalUI ? "TRUE" : "FALSE";
                    var EnableIngameChat = Globals.Options.IngameChat ? "TRUE" : "FALSE";
                    ExecuteInternal("SYN_FILE_PATH|" + Path.Combine(SynapseDir, "workspace") + "|" + EnableUnlock +
                                    "|FALSE|" + EnableInternalUI + "|" + EnableIngameChat);
                    AttachEventInternal?.Invoke(SynAttachEvents.READY);
                }
                else if (Input == "SYN_REATTACH_READY")
                {
                    AttachEventInternal?.Invoke(SynAttachEvents.READY);
                }
            };

            IsLoaded = true;
            LoadEventInternal(SynLoadEvents.READY, Globals.Version);

            Thread.Sleep(1500);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private string GetPipeName(string PipeName)
        {
            return Utils.Sha512(PipeName + RbxId).ToLower().Substring(0, 16);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        protected bool ReadyInternal()
        {
            var ProcList = Process.GetProcessesByName("RobloxPlayerBeta");
            return ProcList.Length != 0 && ProcList[0].Id == RbxId;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static void SendData(string PipeName, string data, int timeout = 0)
        {
            NamedPipeClientStream namedPipeClientStream = null;
            try
            {
                namedPipeClientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                namedPipeClientStream.Connect(timeout);
                using (var streamWriter = new StreamWriter(namedPipeClientStream))
                {
                    streamWriter.Write(data);
                }
            }
            catch (TimeoutException)
            {
            }
            finally
            {
                namedPipeClientStream?.Dispose();
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        protected void ExecuteInternal(string data)
        {
            if (data.Length == 0) return;

            var ProcList = Process.GetProcessesByName("RobloxPlayerBeta");
            if (ProcList.Length == 0 || ProcList[0].Id != RbxId)
            {
                AttachEventInternal?.Invoke(SynAttachEvents.NOT_INJECTED);
                return;
            }

            SendData(GetPipeName("SynapseScript"), data, 50);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public void InlineAutoUpdate()
        {
            AttachEventInternal?.Invoke(SynAttachEvents.NOT_RUNNING_LATEST_VER_UPDATING);

            var Data = WebInterface.GetData();

#if USE_UPDATE_CHECKS
            if (!Data.IsUpdated)
            {
                IsInlineUpdating = false;
                AttachEventInternal?.Invoke(SynAttachEvents.NOT_UPDATED);
                return;
            }
#endif

            var DllName = SynapseDir + "bin\\" + Utils.CreateFileName("Synapse.dll");
            var LauncherName = SynapseDir + "bin\\" + Utils.CreateFileName("Synapse Launcher.exe");

            using (var WC = new WebClient())
            {
#if USE_UPDATE_CHECKS
                try
                {
                    var Salt = Utils.Sha512Bytes(Environment.MachineName + Data.Version);

                    if (!File.Exists(DllName))
                    {
                        AttachEventInternal?.Invoke(SynAttachEvents.UPDATING_DLLS);
                        WC.DownloadFile(Globals.Options.BetaRelease ? Data.BetaDllDownload : Data.DllDownload, DllName);
                        Utils.AppendAllBytes(DllName, Salt);
                    }

                    if (Utils.Sha512Dll(DllName) != (Globals.Options.BetaRelease ? Data.BetaDllHash : Data.DllHash))
                    {
                        AttachEventInternal?.Invoke(SynAttachEvents.UPDATING_DLLS);
                        File.Delete(DllName);
                        WC.DownloadFile(Globals.Options.BetaRelease ? Data.BetaDllDownload : Data.DllDownload, DllName);
                        Utils.AppendAllBytes(DllName, Salt);

                        if (Utils.Sha512Dll(DllName) != (Globals.Options.BetaRelease ? Data.BetaDllHash : Data.DllHash))
                        {
                            File.Delete(DllName);

                            IsInlineUpdating = false;
                            AttachEventInternal?.Invoke(SynAttachEvents.FAILED_TO_UPDATE);
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    IsInlineUpdating = false;
                    AttachEventInternal?.Invoke(SynAttachEvents.FAILED_TO_UPDATE);
                    return;
                }
#endif
                try
                {
                    if (!File.Exists(SynapseDir + "bin\\Monaco.html"))
                    {
                        WC.DownloadFile("https://cdn.synapse.to/synapsedistro/distro/Monaco.zip", SynapseDir + "bin\\Monaco.zip");

                        ZipFile.ExtractToDirectory(SynapseDir + "bin\\Monaco.zip", SynapseDir + "bin");
                        File.Delete(SynapseDir + "bin\\Monaco.zip");
                    }

                    if (!File.Exists(SynapseDir + "bin\\CefSharp.dll"))
                    {
                        WC.DownloadFile(Data.CefSharpDownload, SynapseDir + "bin\\CefSharp.zip");

                        if (Utils.Sha512(SynapseDir + "bin\\CefSharp.zip", true) != Data.CefSharpHash)
                        {
                            File.Delete(SynapseDir + "bin\\CefSharp.zip");

                            IsInlineUpdating = false;
                            AttachEventInternal?.Invoke(SynAttachEvents.FAILED_TO_UPDATE);
                            return;
                        }

                        ZipFile.ExtractToDirectory(SynapseDir + "bin\\CefSharp.zip", SynapseDir + "bin");
                        File.Delete(SynapseDir + "bin\\CefSharp.zip");
                    }
                }
                catch (Exception)
                {
                    IsInlineUpdating = false;
                    AttachEventInternal?.Invoke(SynAttachEvents.FAILED_TO_UPDATE);
                    return;
                }

                try
                {
                    if (!Directory.Exists("bin\\x64")) Directory.CreateDirectory("bin\\x64");
                    if (!Directory.Exists("bin\\x86")) Directory.CreateDirectory("bin\\x86");
                    if (!Directory.Exists("bin\\redis")) Directory.CreateDirectory("bin\\redis");

                    if (!File.Exists("bin\\x64\\SQLite.Interop.dll") || !File.Exists("bin\\x86\\SQLite.Interop.dll") ||
                        !File.Exists("bin\\redis\\D3DCompiler_43.dll") || !File.Exists("bin\\redis\\xinput1_3.dll"))
                    {
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
                    IsInlineUpdating = false;
                    AttachEventInternal?.Invoke(SynAttachEvents.FAILED_TO_UPDATE);
                    return;
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

            Globals.Version = Data.Version;

            AttachEventInternal?.Invoke(SynAttachEvents.REINJECTING, Globals.Version);

            int ProcId;
            if (RobloxIdOverride != 0)
            {
                ProcId = RobloxIdOverride;
                RobloxIdOverride = 0;
            }
            else
            {
                var ProcList = Process.GetProcessesByName("RobloxPlayerBeta");
                if (ProcList.Length == 0)
                {
                    IsInlineUpdating = false;

                    AttachEventInternal?.Invoke(SynAttachEvents.FAILED_TO_FIND);

                    return;
                }

                if (ProcList[0].Id == RbxId)
                {
                    IsInlineUpdating = false;

                    AttachEventInternal?.Invoke(SynAttachEvents.ALREADY_INJECTED);

                    return;
                }

                ProcId = ProcList[0].Id;
            }

            RobloxIdTemp = ProcId;
            CInterface.Inject(Globals.DllPath,
                SynapseDir + "\\bin\\redis\\D3DCompiler_43.dll", SynapseDir + "\\bin\\redis\\xinput1_3.dll",
                ProcId, false);

            IsInlineUpdating = false;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void AttachWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int ProcId;
            if (RobloxIdOverride != 0)
            {
                ProcId = RobloxIdOverride;
                RobloxIdOverride = 0;
            }
            else
            {
                var ProcList = Process.GetProcessesByName("RobloxPlayerBeta");
                if (ProcList.Length == 0)
                {
                    AttachEventInternal?.Invoke(SynAttachEvents.FAILED_TO_FIND);

                    return;
                }

                if (ProcList[0].Id == RbxId)
                {
                    AttachEventInternal?.Invoke(SynAttachEvents.ALREADY_INJECTED);

                    return;
                }

                ProcId = ProcList[0].Id;
            }

            AttachEventInternal?.Invoke(SynAttachEvents.CHECKING);

            if (Globals.Version != VerifyWebsiteWithVersion())
            {
                IsInlineUpdating = true;
                new Thread(InlineAutoUpdate).Start();

                return;
            }

            AttachEventInternal?.Invoke(SynAttachEvents.INJECTING);

            RobloxIdTemp = ProcId;
            CInterface.Inject(Globals.DllPath,
                SynapseDir + "\\bin\\redis\\D3DCompiler_43.dll", SynapseDir + "\\bin\\redis\\xinput1_3.dll",
                ProcId, false);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void HubWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            VerifyWebsite();

            var Data = WebInterface.GetScriptHubData();
            var Out = new List<SynHubEntry>();

            foreach (var Entry in Data.Entries)
            {
                Out.Add(new SynHubEntry(Entry.Url, this)
                {
                    Description = Entry.Description,
                    Name = Entry.Name,
                    Picture = Entry.Picture
                });
            }

            ScriptHubInit = false;
            HubEventInternal?.Invoke(Out);
        }
    }
}
