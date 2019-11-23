using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Synapse_Launcher.Interfaces;
using Synapse_Launcher.Static;
using Path = System.IO.Path;

namespace Synapse_Launcher
{
    public partial class MainWindow
    {
        public static BackgroundWorker LaunchWorker = new BackgroundWorker();

        public delegate void InteractMessageEventHandler(object sender, string Input);
        public event InteractMessageEventHandler InteractMessageRecieved;

        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [Flags]
        public enum ProcessCreationFlags : uint
        {
            ZERO_FLAG = 0x00000000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }

        [DllImport("kernel32.dll")]
        public static extern bool CreateProcess(string lpApplicationName,
            string lpCommandLine, IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles, ProcessCreationFlags dwCreationFlags,
            IntPtr lpEnvironment, string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern uint SuspendThread(IntPtr hThread);

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
                        Clipboard.SetText(((Exception) args.ExceptionObject).ToString());
                    });

                STAThread.SetApartmentState(ApartmentState.STA);
                STAThread.Start();
                STAThread.Join();

                Thread.Sleep(1000);
            };

            LaunchWorker.DoWork += LaunchWorker_DoWork;

            InitializeComponent();
        }

        public static readonly Random Rnd = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Rnd.Next(s.Length)]).ToArray());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = RandomString(Rnd.Next(10, 32));

            LaunchWorker.RunWorkerAsync();
        }

        private static string DecodeUrlString(string Url)
        {
            string NewUrl;
            while ((NewUrl = HttpUtility.UrlDecode(Url)) != Url)
                Url = NewUrl;
            return NewUrl;
        }

        private static string CombineArgs(IEnumerable<string> Arg)
        {
            var Ret = Arg.Where((T, I) => I != 0).Aggregate("", (Current, T) => Current + T + ":");

            return Ret.TrimEnd(':');
        }

        public void SetStatusText(string Status, int Percentage)
        {
            Dispatcher.Invoke(() =>
            {
                StatusBox.Content = Status;
                ProgressBox.Value = Percentage;
            });
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public bool SendData(string PipeName, string data, int timeout = 0)
        {
            NamedPipeClientStream namedPipeClientStream = null;
            try
            {
                namedPipeClientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                namedPipeClientStream.Connect(timeout);
                using (var streamWriter = new StreamWriter(namedPipeClientStream))
                {
                    streamWriter.Write(data);
                    streamWriter.Flush();
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                namedPipeClientStream?.Dispose();
            }

            return true;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public string GetPipeName(int pid, string PipeName)
        {
            return Utils.Sha512(PipeName + pid).ToLower().Substring(0, 16);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public void Execute(int pid, string data)
        {
            if (data.Length == 0) return;

            SendData(GetPipeName(pid, "SynapseScript"), data, 50);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void LaunchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SetStatusText("Loading Synapse...", 25);

            var BaseDirectory = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .FullName;

            try
            {
                Directory.SetCurrentDirectory(BaseDirectory);
                CInterface.Init();
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

            SetStatusText("Getting version...", 50);

            var RawArgs = Environment.GetCommandLineArgs();
            var DecodedArgs = DecodeUrlString(RawArgs[1]).Split(' ');
            var Args = DecodedArgs.Select(Arg => Arg.Split(':')).ToDictionary(Split => Split[0], CombineArgs);

            int SecureSeed;

            using (var RNG = new RNGCryptoServiceProvider())
            {
                var Data = new byte[4];
                RNG.GetBytes(Data);

                SecureSeed = BitConverter.ToInt32(Data, 0);
            }

            var SpoofBrowserTracker = new Random(SecureSeed).Next();
            var UnixEpoch =
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var UnixTime = (long) (DateTime.UtcNow - UnixEpoch).TotalMilliseconds;

            string CurrentVersion;
            using (var WC = new WebClient())
            {
                CurrentVersion =
                    WC.DownloadString(
                            "https://versioncompatibility.api.roblox.com/GetCurrentClientVersionUpload/?apiKey=76e5a40c-3ae1-4028-9f10-7c62520bd94f&binaryType=WindowsPlayer")
                        .Replace("\"", "");
            }

            SetStatusText("Launching...", 75);

            var VersionFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"Roblox\\Versions\\{CurrentVersion}\\");
            if (!Directory.Exists(VersionFolder))
            {
                VersionFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), $"Roblox\\Versions\\{CurrentVersion}\\");
                if (!Directory.Exists(VersionFolder))
                {
                    VersionFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), $"Roblox\\Versions\\{CurrentVersion}\\");
                    if (!Directory.Exists(VersionFolder))
                    {
                        if (!DataInterface.Exists("launcherbackup") || !File.Exists(DataInterface.Read<string>("launcherbackup")))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show(
                                    "Synapse X has detected a Roblox update and could not find its backup executable. Please reinstall Roblox.",
                                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Warning);
                            });

                            Environment.Exit(0);
                            return;
                        }

                        Process.Start(DataInterface.Read<string>("launcherbackup"), RawArgs[1]);
                        Environment.Exit(0);
                        return;
                    }
                }
            }

            var SI = new STARTUPINFO();
            var Suc = CreateProcess(Path.Combine(VersionFolder, "RobloxPlayerBeta.exe"),
                $"--play -a https://www.roblox.com/Login/Negotiate.ashx -t {Args["gameinfo"]} -j {Args["placelauncherurl"].Replace(Args["browsertrackerid"], SpoofBrowserTracker.ToString())} -b {SpoofBrowserTracker} --launchtime={UnixTime} --rloc {Args["robloxLocale"]} --gloc {Args["gameLocale"]}",
                IntPtr.Zero, IntPtr.Zero, false,
                ProcessCreationFlags.CREATE_SUSPENDED,
                IntPtr.Zero, null, ref SI, out var PI);

            if (Suc)
            {
                if (!SendData("SynapseLaunch", "SYN_LAUNCH_NOTIIFCATION|" + PI.dwProcessId))
                {
                    DataInterface.Save("savedpid", new Data.SavedPid
                    {
                        Pid = Convert.ToInt32(PI.dwProcessId),
                        StartTime = Process.GetProcessById(Convert.ToInt32(PI.dwProcessId)).StartTime
                    });

                    StreamReader InteractReader;

                    new Thread(() =>
                    {
                        try
                        {
                            var PS = new PipeSecurity();
                            var Rule = new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                                PipeAccessRights.ReadWrite, AccessControlType.Allow);
                            PS.AddAccessRule(Rule);
                            var Server = new NamedPipeServerStream("SynapseInteract", PipeDirection.InOut, 1,
                                PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, PS);
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
                                if (Line != "SYN_READY" && Line != "SYN_REATTACH_READY" && Line != "SYN_INTERRUPT")
                                    continue;

                                Thread.Sleep(1500);
                                Environment.Exit(0);
                            }
                        }
                        catch (Exception)
                        {
                            Environment.Exit(0);
                        }
                    }).Start();

                    InteractMessageRecieved += delegate (object Sender, string Input)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            switch (Input)
                            {
                                case "SYN_CHECK_WL":
                                    SetStatusText("Checking whitelist...", 80);
                                    break;
                                case "SYN_SCANNING":
                                    SetStatusText("Scanning...", 95);
                                    break;
                                case "SYN_INTERRUPT":
                                    Environment.Exit(0);
                                    break;
                                case "SYN_READY":
                                {
                                    SetStatusText("Ready!", 100);
                                    break;
                                }
                            }
                        });

                        if (Input == "SYN_READY")
                        {
                            var Read = DataInterface.Read<Data.OptionsHolder>("options");
                            var Options = JsonConvert.DeserializeObject<Data.Options>(Read.Data);

                            var EnableUnlock = Options.UnlockFPS ? "TRUE" : "FALSE";
                            var EnableInternalUI = Options.InternalUI ? "TRUE" : "FALSE";
                            var EnableIngameChat = Options.IngameChat ? "TRUE" : "FALSE";
                            Execute(Convert.ToInt32(PI.dwProcessId), "SYN_FILE_PATH|" + Path.Combine(BaseDirectory, "workspace") + "|" + EnableUnlock + "|FALSE|" + EnableInternalUI + "|" + EnableIngameChat);

                            Thread.Sleep(1500);
                            Environment.Exit(0);
                        }
                    };

                    CInterface.Inject(BaseDirectory + "\\bin\\" + Utils.CreateFileName("Synapse.dll"),
                        BaseDirectory + "\\bin\\redis\\D3DCompiler_43.dll",
                        BaseDirectory + "\\bin\\redis\\xinput1_3.dll", Convert.ToInt32(PI.dwProcessId), true);
                    ResumeThread(PI.hThread);

                    return;
                }

                CInterface.Inject(BaseDirectory + "\\bin\\" + Utils.CreateFileName("Synapse.dll"),
                    BaseDirectory + "\\bin\\redis\\D3DCompiler_43.dll", BaseDirectory + "\\bin\\redis\\xinput1_3.dll",
                    Convert.ToInt32(PI.dwProcessId), true);
                ResumeThread(PI.hThread);
            }

            SetStatusText("Ready!", 100);

            Thread.Sleep(1500);
            Environment.Exit(0);
        }
    }
}
