#define USE_UPDATE_CHECKS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Net;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;
using Microsoft.Win32;
using Synapse_UI_WPF.Controls;
using Synapse_UI_WPF.Interfaces;
using Synapse_UI_WPF.Static;
using Synapse_UI_WPF.Watcher;
using Process = System.Diagnostics.Process;

namespace Synapse_UI_WPF
{
    public partial class MainWindow
    {
        public ProcessWatcher Watcher;

        public delegate void InteractMessageEventHandler(object sender, string Input);
        public event InteractMessageEventHandler InteractMessageRecieved;

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private int RobloxIdOverride;
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private int RobloxIdTemp;
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static int RbxId;

        public bool OptionsOpen;

        public bool ScriptHubOpen;
        private bool ScriptHubInit;
        public bool IsInlineUpdating;

        private static ThemeInterface.TAttachStrings AttachStrings;

        private readonly string BaseDirectory;
        private readonly string ScriptsDirectory;

        public static BackgroundWorker Worker = new BackgroundWorker();
        public static BackgroundWorker HubWorker = new BackgroundWorker();

        public MainWindow()
        {
			Cef.EnableHighDPISupport();
			var settings = new CefSettings();
			settings.SetOffScreenRenderingBestPerformanceArgs();
			Cef.Initialize(settings);

			InitializeComponent();

			Worker.DoWork += Worker_DoWork;
            HubWorker.DoWork += HubWorker_DoWork;

            StreamReader InteractReader = null;
            StreamReader LaunchReader;

            try
            {
                Watcher = new ProcessWatcher("RobloxPlayerBeta.exe");
                Watcher.ProcessCreated += Proc =>
                {
                    if (!Globals.Options.AutoAttach || Globals.Options.AutoLaunch) return;
                    RobloxIdOverride = Convert.ToInt32(Proc.ProcessId);
                    Worker.RunWorkerAsync();
                };
                Watcher.ProcessDeleted += Proc =>
                {
                    if (Proc.ProcessId == RobloxIdTemp)
                    {
                        InteractReader?.Close();
                    }
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

            var TMain = Globals.Theme.Main;
            ThemeInterface.ApplyWindow(this, TMain.Base);
            ThemeInterface.ApplyLogo(IconBox, TMain.Logo);
            ThemeInterface.ApplySeperator(TopBox, TMain.TopBox);
            ThemeInterface.ApplyFormatLabel(TitleBox, TMain.TitleBox, Globals.Version);
            ThemeInterface.ApplyListBox(ScriptBox, TMain.ScriptBox);
            ThemeInterface.ApplyButton(MiniButton, TMain.MinimizeButton);
            ThemeInterface.ApplyButton(CloseButton, TMain.ExitButton);
            ThemeInterface.ApplyButton(ExecuteButton, TMain.ExecuteButton);
            ThemeInterface.ApplyButton(ClearButton, TMain.ClearButton);
            ThemeInterface.ApplyButton(OpenFileButton, TMain.OpenFileButton);
            ThemeInterface.ApplyButton(ExecuteFileButton, TMain.ExecuteFileButton);
            ThemeInterface.ApplyButton(SaveFileButton, TMain.SaveFileButton);
            ThemeInterface.ApplyButton(OptionsButton, TMain.OptionsButton);
            ThemeInterface.ApplyButton(AttachButton, TMain.AttachButton);
            ThemeInterface.ApplyButton(ScriptHubButton, TMain.ScriptHubButton);

            ScaleTransform.ScaleX = Globals.Options.WindowScale;
            ScaleTransform.ScaleY = Globals.Options.WindowScale;

            AttachStrings = TMain.BaseStrings;

            BaseDirectory = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;

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
                    if (string.IsNullOrWhiteSpace(Line)) Line = "SYN_INTERRUPT";
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

            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(15000);

                    try
                    {
                        var EditorText = "";
                        Dispatcher.Invoke(() => { EditorText = Browser.GetText(); });

                        DataInterface.Save("savedws", EditorText);
                    }
                    catch (Exception) { }
                }
            }).Start();

            InteractMessageRecieved += delegate (object Sender, string Input)
            {
                Dispatcher.Invoke(() =>
                {
                    switch (Input)
                    {
                        case "SYN_CHECK_WL":
                            SetTitle(AttachStrings.CheckingWhitelist);
                            break;
                        case "SYN_SCANNING":
                            SetTitle(AttachStrings.Scanning);
                            break;
                        case "SYN_INTERRUPT":
                            SetTitle(" (failed to attach!)", 3000);
                            break;
                        case "SYN_READY":
                        case "SYN_REATTACH_READY":
                        {
                            SetTitle(AttachStrings.Ready, 3000);
                            break;
                        }
                    }
                });

                if (Input == "SYN_READY")
                {
                    var ProcList = Process.GetProcessesByName("RobloxPlayerBeta");
                    RbxId = RobloxIdTemp == 0 ? ProcList[0].Id : RobloxIdTemp;
                    RobloxIdTemp = 0;
                    var EnableUnlock = Globals.Options.UnlockFPS ? "TRUE" : "FALSE";
                    var EnableWebSocket = TMain.WebSocket.Enabled ? "TRUE" : "FALSE";
                    var EnableInternalUI = Globals.Options.InternalUI ? "TRUE" : "FALSE";
                    var EnableIngameChat = Globals.Options.IngameChat ? "TRUE" : "FALSE";
                    Execute("SYN_FILE_PATH|" + Path.Combine(BaseDirectory, "workspace") + "|" + EnableUnlock + "|" + EnableWebSocket + "|" + EnableInternalUI + "|" + EnableIngameChat);
                }
            };

            ScriptsDirectory = Path.Combine(BaseDirectory, "scripts");

            foreach (var FilePath in Directory.GetFiles(ScriptsDirectory))
            {
                ScriptBox.Items.Add(Path.GetFileName(FilePath));
            }

            if (TMain.WebSocket.Enabled)
            {
                WebSocketInterface.Start(24892, this);
            }
        }

        public void SetTitle(string Str, int Delay = 0)
        {
            Dispatcher.Invoke(() =>
            {
                TitleBox.Content =
                    ThemeInterface.ConvertFormatString(Globals.Theme.Main.TitleBox, Globals.Version) + Str;
            });

            if (Delay != 0)
            {
                new Thread(() =>
                {
                    Thread.Sleep(Delay);
                    Dispatcher.Invoke(() =>
                    {
                        TitleBox.Content =
                            ThemeInterface.ConvertFormatString(Globals.Theme.Main.TitleBox, Globals.Version);
                    });
                }).Start();
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public string GetPipeName(string PipeName)
        {
            return Utils.Sha512(PipeName + RbxId).ToLower().Substring(0, 16);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public bool Ready()
        {
            var ProcList = Process.GetProcessesByName("RobloxPlayerBeta");
            return ProcList.Length != 0 && ProcList[0].Id == RbxId;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static void SendData(string PipeName, string data, int timeout = 0)
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
        public void Execute(string data)
        {
            if (data.Length == 0) return;

            var ProcList = Process.GetProcessesByName("RobloxPlayerBeta");
            if (ProcList.Length == 0 || ProcList[0].Id != RbxId)
            {
                SetTitle(AttachStrings.NotInjected, 3000);
                return;
            }

            SendData(GetPipeName("SynapseScript"), data, 50);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = WebInterface.RandomString(WebInterface.Rnd.Next(10, 32));
        }

        private void Browser_MonacoReady()
        {
            Browser.SetTheme(Globals.Theme.Main.Editor.Light ? MonacoTheme.Light : MonacoTheme.Dark);

            var SavedWS = "";
            try
            {
                if (DataInterface.Exists("savedws")) SavedWS = DataInterface.Read<string>("savedws");
            }
            catch (Exception)
            {
                DataInterface.Save("savedws", "");
            }

            Browser.SetText(SavedWS);

            /* Intellisense */

            var KeywordsControlFlow = new List<string>
            {
                "and", "do", "elseif",
                "for", "function", "if",
                "in", "local", "not", "or",
                "then", "until", "while"
            };

            var KeywordsValue = new List<string>
            {
                "_G", "shared", "true", "false", "nil", "end",
                "break", "else", "repeat", "then", "return"
            };

            var IntellisenseNoDocs = new List<string>
            {
                "error", "getfenv", "getmetatable",
                "newproxy", "next", "pairs",
                "pcall", "print", "rawequal", "rawget", "rawset", "select", "setfenv",
                "tonumber", "tostring", "type", "unpack", "xpcall", "_G",
                "shared", "delay", "require", "spawn", "tick", "typeof", "wait", "warn",
                "game", "Enum", "script", "workspace"
            };

            foreach (var Key in KeywordsControlFlow)
            {
                Browser.AddIntellisense(Key, "Keyword", "", Key + " ");
            }

            foreach (var Key in KeywordsValue)
            {
                Browser.AddIntellisense(Key, "Keyword", "", Key);
            }

            foreach (var Key in IntellisenseNoDocs)
            {
                Browser.AddIntellisense(Key, "Method", "", Key);
            }

            Browser.AddIntellisense("hookfunction(<function> old, <function> hook)", "Method",
                "Hooks function 'old', replacing it with the function 'hook'. The old function is returned, you must use it to call the function further.",
                "hookfunction");
            Browser.AddIntellisense("getgenv(<void>)", "Method",
                "Returns the environment that will be applied to each script ran by Synapse.",
                "getgenv");
            Browser.AddIntellisense("keyrelease(<int> key)", "Method",
                "Releases 'key' on the keyboard. You can access the int key values on MSDN.",
                "keyrelease");
            Browser.AddIntellisense("setclipboard(<string> value)", "Method",
                "Sets 'value' to the clipboard.",
                "setclipboard");
            Browser.AddIntellisense("mouse2press(<void>)", "Method",
                "Clicks down on the right mouse button.",
                "mouse2press");
            Browser.AddIntellisense("getsenv(<LocalScript, ModuleScript> Script)", "Method",
                "Returns the environment of Script. Returns nil if the script is not running.",
                "getsenv");
            Browser.AddIntellisense("checkcaller(<void>)", "Method",
                "Returns true if the current thread was made by Synapse. Useful for metatable hooks.",
                "checkcaller");

            Browser.AddIntellisense("bit", "Class", "Bit Library", "bit");
            Browser.AddIntellisense("bit.bdiv(<uint> dividend, <uint> divisor)", "Method",
                "Divides 'dividend' by 'divisor', remainder is not returned.",
                "bit.bdiv");
            Browser.AddIntellisense("bit.badd(<uint> a, <uint> b)", "Method",
                "Adds 'a' with 'b', allows overflows (unlike normal Lua).",
                "bit.badd");
            Browser.AddIntellisense("bit.bsub(<uint> a, <uint> b)", "Method",
                "Subtracts 'a' with 'b', allows overflows (unlike normal Lua).",
                "bit.badd");
            Browser.AddIntellisense("bit.rshift(<uint> val, <uint> by)", "Method",
                "Does a right shift on 'val' using 'by'.",
                "bit.rshift");
            Browser.AddIntellisense("bit.band(<uint> val, <uint> by)", "Method",
                "Does a logical AND (&) on 'val' using 'by'.",
                "bit.band");
            Browser.AddIntellisense("bit.bor(<uint> val, <uint> by)", "Method",
                "Does a logical OR (|) on 'val' using 'by'.",
                "bit.bor");
            Browser.AddIntellisense("bit.bxor(<uint> val, <uint> by)", "Method",
                "Does a logical XOR (^) on 'val' using 'by'.",
                "bit.bxor");
            Browser.AddIntellisense("bit.bnot(<uint> val)", "Method",
                "Does a logical NOT on 'val'.",
                "bit.bnot");
            Browser.AddIntellisense("bit.bmul(<uint> val, <uint> by)", "Method",
                "Multiplies 'val' using 'by', allows overflows (unlike normal Lua)",
                "bit.bmul");
            Browser.AddIntellisense("bit.bswap(<uint> val)", "Method",
                "Does a bitwise swap on 'val'.",
                "bit.bswap");
            Browser.AddIntellisense("bit.tobit(<uint> val)", "Method",
                "Converts 'val' into proper form for bitwise operations.",
                "bit.tobit");
            Browser.AddIntellisense("bit.ror(<uint> val, <uint> by)", "Method",
                "Rotates right 'val' using 'by'.",
                "bit.ror");
            Browser.AddIntellisense("bit.lshift(<uint> val, <uint> by)", "Method",
                "Does a left shift on 'val' using 'by'.",
                "bit.lshift");
            Browser.AddIntellisense("bit.tohex(<uint> val)", "Method",
                "Converts 'val' to a hex string.",
                "bit.tohex");

            Browser.AddIntellisense("debug", "Class", "Debug Library", "debug");
            Browser.AddIntellisense("debug.getconstant(<function, int> fi, <int> idx)", "Method", "Returns the constant at index 'idx' in function 'fi' or level 'fi'.", "debug.getconstant");
            Browser.AddIntellisense("debug.profilebegin(<string> label>", "Method", "Opens a microprofiler label.", "debug.profilebegin");
            Browser.AddIntellisense("debug.profileend(<void>)", "Method", "Closes the top microprofiler label.", "debug.profileend");
            Browser.AddIntellisense("debug.traceback(<void>)", "Method", "Returns a traceback of the current stack as a string.", "debug.traceback");
            Browser.AddIntellisense("debug.getfenv(<T> o)", "Method", "Returns the environment of object 'o'.", "debug.getfenv");
            Browser.AddIntellisense("debug.getupvalue(<function, int> fi, <string> upval)", "Method", "Returns the upvalue with name 'upval' in function or level 'fi'.", "debug.getupvalue");
            Browser.AddIntellisense("debug.getlocals(<int> lvl)", "Method", "Returns a table containing the upvalues at level 'lvl'.", "debug.getlocals");
            Browser.AddIntellisense("debug.setmetatable(<T> o, <table> mt)", "Method", "Set the metatable of 'o' to 'mt'.", "debug.setmetatable");
            Browser.AddIntellisense("debug.getconstants(<function, int> fi)", "Method", "Retrieve the constants in function 'fi' or at level 'fi'.", "debug.getconstants");
            Browser.AddIntellisense("debug.getupvalues(<function, int> fi)", "Method", "Retrieve the upvalues in function 'fi' or at level 'fi'.", "debug.getupvalues");
            Browser.AddIntellisense("debug.setlocal(<int> lvl, <string> localname, <T> value)", "Method", "Set local 'localname' to value 'value' at level 'lvl'.", "debug.setlocal");
            Browser.AddIntellisense("debug.setupvalue(<function, int> fi, <string> upvname, <T> value)", "Method", "Set upvalue 'upvname' to value 'value' at level or function 'fi'.", "debug.setupvalue");
            Browser.AddIntellisense("debug.setconstant(<function, int> fi, <string> consname, <int, bool, nil, string> value)", "Method", "Set constant 'consname' to tuple 'value' at level or function 'fi'.", "debug.setupvalue");
            Browser.AddIntellisense("debug.getregistry(<void>)", "Method", "Returns the registry", "debug.getregistry");
            Browser.AddIntellisense("debug.getinfo(<function, int> fi, <string> w)", "Method", "Returns a table of info pertaining to the Lua function 'fi'.", "debug.getinfo");
            Browser.AddIntellisense("debug.getlocal(<int> lvl, <string> localname)", "Method", "Returns the local with name 'localname' in level 'lvl'.", "debug.getlocal");

            Browser.AddIntellisense("loadfile(<string> path)", "Method", "Loads in the contents of a file as a chunk and returns it if compilation is successful. Otherwise, if an error has occured during compilation, nil followed by the error message will be returned.", "loadfile");
            Browser.AddIntellisense("loadstring(<string> chunk, [<string> chunkname])", "Method", "Loads 'chunk' as a Lua function and returns it if compilation is succesful. Otherwise, if an error has occured during compilation, nil followed by the error message will be returned.", "loadstring");
            Browser.AddIntellisense("writefile(<string> filepath, <string> contents)", "Method", "Writes 'contents' to the supplied filepath.", "writefile");
            Browser.AddIntellisense("mousescroll(<signed int> px)", "Method", "Scrolls the mouse wheel virtually by 'px' pixels.", "mousescroll");
            Browser.AddIntellisense("mouse2click(<void>)", "Method", "Virtually presses the right mouse button.", "mouse2click");
            Browser.AddIntellisense("islclosure(<function> f)", "Method", "Returns true if 'f' is an LClosure", "islclosure");
            Browser.AddIntellisense("mouse1press(<void>)", "Method", "Simulates a left mouse button press without releasing it.", "mouse1press");
            Browser.AddIntellisense("mouse1release(<void>)", "Method", "Simulates a left mouse button release.", "mouse1release");
            Browser.AddIntellisense("keypress(<int> keycode)", "Method", "Simulates a key press for the specified keycode. For more information: https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes", "keypress");
            Browser.AddIntellisense("mouse2release(<void>)", "Method", "Simulates a right mouse button release.", "mouse2release");
            Browser.AddIntellisense("newcclosure(<function> f)", "Method", "Pushes a new c closure that invokes function 'f' upon call. Used for metatable hooks.", "newcclosure");
            Browser.AddIntellisense("getinstances(<void>)", "Method", "Returns a list of all instances within the game.", "getinstances");
            Browser.AddIntellisense("getnilinstances(<void>)", "Method", "Returns a list of all instances parented to nil within the game.", "getnilinstances");
            Browser.AddIntellisense("readfile(<string> path)", "Method", "Reads the contents of the file located at 'path' and returns it. If the file does not exist, it errors.", "readfile");
            Browser.AddIntellisense("getscripts(<void>)", "Method", "Returns a list of all scripts within the game.", "getscripts");
            Browser.AddIntellisense("getrunningscripts(<void>)", "Method", "Returns a list of all scripts currently running.", "getrunningscripts");
            Browser.AddIntellisense("appendfile(<string> path, <string> content)", "Method", "Appends 'content' to the file contents at 'path'. If the file does not exist, it errors", "appendfile");
            Browser.AddIntellisense("listfiles(<string> folder)", "Method", "Returns a table of files in 'folder'.", "listfiles");
            Browser.AddIntellisense("isfile(<string> path)", "Method", "Returns if 'path' is a file or not.", "isfile");
            Browser.AddIntellisense("isfolder(<string> path)", "Method", "Returns if 'path' is a folder or not.", "isfolder");
            Browser.AddIntellisense("delfolder(<string> path)", "Method", "Deletes 'folder' in the workspace directory.", "delfolder");
            Browser.AddIntellisense("delfile(<string> path)", "Method", "Deletes 'file' from the workspace directory.", "delfile");
            Browser.AddIntellisense("getreg(<void>)", "Method", "Returns the Lua registry.", "getreg");
            Browser.AddIntellisense("getgc(<void>)", "Method", "Returns a copy of the Lua GC list.", "getgc");
            Browser.AddIntellisense("mouse1click(<void>)", "Method", "Simulates a full left mouse button press.", "mouse1click");
            Browser.AddIntellisense("getrawmetatable(<T> value)", "Method", "Retrieve the metatable of value irregardless of value's metatable's __metatable field. Returns nil if it doesn't exist.", "getrawmetatable");
            Browser.AddIntellisense("setreadonly(<table> table, <bool> ro)", "Method", "Sets table's read-only value to ro", "setreadonly");
            Browser.AddIntellisense("isreadonly(<table> table)", "Method", "Returns table's read-only condition.", "isreadonly");
            Browser.AddIntellisense("getrenv(<void>)", "Method", "Returns the global Roblox environment for the LocalScript state.", "getrenv");
            Browser.AddIntellisense("decompile(<LocalScript, ModuleScript, function> Script, bool Bytecode = false)", "Method", "Decompiles Script and returns the decompiled script. If the decompilation fails, then the return value will be an error message.", "decompile");
            Browser.AddIntellisense("dumpstring(<string> Script)", "Method", "Returns the Roblox formatted bytecode for source string 'Script'.", "dumpstring");
            Browser.AddIntellisense("getloadedmodules(<void>)", "Method", "Returns all ModuleScripts loaded in the game.", "getloadedmodules");
            Browser.AddIntellisense("isrbxactive(<void>)", "Method", "Returns if the Roblox window is in focus.", "getloadedmodules");
            Browser.AddIntellisense("getcallingscript(<void>)", "Method", "Gets the script that is calling this function.", "getcallingscript");
            Browser.AddIntellisense("setnonreplicatedproperty(<Instance> obj, <string> prop, <T> value)", "Method", "Sets the prop property of obj, not replicating to the server. Useful for anticheat bypasses.", "setnonreplicatedproperty");
            Browser.AddIntellisense("getconnections(<Signal> obj)", "Method", "Gets a list of connections to the specified signal. You can then use :Disable and :Enable on the connections to disable/enable them.", "getconnections");
            Browser.AddIntellisense("getspecialinfo(<Instance> obj)", "Method", "Gets a list of special properties for MeshParts, UnionOperations, and Terrain.", "getspecialinfo");
            Browser.AddIntellisense("messagebox(<string> message, <string> title, <int> options)", "Method", "Makes a MessageBox with 'message', 'title', and 'options' as options. See https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-messagebox for more information.", "messagebox");
            Browser.AddIntellisense("messageboxasync(<string> message, <string> title, <int> options)", "Method", "Makes a MessageBox with 'message', 'title', and 'options' as options. See https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-messagebox for more information.", "messageboxasync");
            Browser.AddIntellisense("rconsolename(<string> title)", "Method", "Sets the currently allocated console title to 'title'.", "rconsolename");
            Browser.AddIntellisense("rconsoleinput(<void>)", "Method", "Yields until the user inputs information into ther console. Returns the input the put in.", "rconsoleinput");
            Browser.AddIntellisense("rconsoleinputasync(<void>)", "Method", "Yields until the user inputs information into ther console. Returns the input the put in.", "rconsoleinputasync");
            Browser.AddIntellisense("rconsoleprint(<string> message)", "Method", "Prints 'message' into the console.", "rconsoleprint");
            Browser.AddIntellisense("rconsoleinfo(<string> message)", "Method", "Prints 'message' into the console, with a info text before it.", "rconsoleinfo");
            Browser.AddIntellisense("rconsolewarn(<string> message)", "Method", "Prints 'message' into the console, with a warning text before it.", "rconsolewarn");
            Browser.AddIntellisense("rconsoleerr(<string> message)", "Method", "Prints 'message' into the console, with a error text before it.", "rconsoleerr");
            Browser.AddIntellisense("fireclickdetector(<ClickDetector> detector, <number, nil> distance)", "Method", "Fires click detector 'detector' with 'distance'. If a distance is not provided, it will be 0.", "fireclickdetector");
            Browser.AddIntellisense("firetouchinterest(<Part> part, <TouchTransmitter> transmitter, <number> toggle)", "Method", "Fires touch 'transmitter' with 'part'. Use 0 to toggle it being touched, 1 for it not being toggled.", "firetouchinterest");
            Browser.AddIntellisense("saveinstance(<table> t)", "Method", "Saves the Roblox game into your workspace folder.", "saveinstance");

            Browser.AddIntellisense("syn", "Class", "Synapse X Library", "syn");
            Browser.AddIntellisense("syn.crypt.encrypt(<string> data, <string> key)", "Method", "Encrypt's data with key.", "syn.crypt.encrypt");
            Browser.AddIntellisense("syn.crypt.decrypt(<string> data, <string> key)", "Method", "Decrypt's data with key.", "syn.crypt.decrypt");
            Browser.AddIntellisense("syn.crypt.hash(<string> data)", "Method", "Hashes data.", "syn.crypt.decrypt");
            Browser.AddIntellisense("syn.crypt.base64.encode(<string> data)", "Method", "Encodes data with bas64.", "syn.crypt.base64.encode");
            Browser.AddIntellisense("syn.crypt.base64.decode(<string> data)", "Method", "Decodes data with bas64.", "syn.crypt.base64.encode");
            Browser.AddIntellisense("syn.cache_replace(<Instance> obj, <Instance> t_obj)", "Method", "Replace obj in the cache with t_obj.", "syn.cache_replace");
            Browser.AddIntellisense("syn.cache_invalidate(<Instance> obj)", "Method", "Invalidate obj's cache entry, forcing a recache upon the next lookup.", "syn.invalidate_cache");
            Browser.AddIntellisense("syn.set_thread_identity(<int> n)", "Method", "Sets the current thread identity after a Task Scheduler cycle is performed. (call wait() after invoking this function for the expected results)", "syn.set_thread_identity");
            Browser.AddIntellisense("syn.get_thread_identity(<void>)", "Method", "Returns the current thread identity.", "syn.get_thread_identity");
            Browser.AddIntellisense("syn.is_cached(<Instance> obj)", "Method", "Returns true if the instance is currently cached within the registry.", "syn.is_cached");
            Browser.AddIntellisense("syn.write_clipboard(<string> content)", "Method", "Writes 'content' to the current Windows clipboard.", "syn.write_clipboard");
            Browser.AddIntellisense("mousemoverel(<int> x, <int> y)", "Method", "Moves the mouse cursor relatively to the current mouse position by coordinates 'x' and 'y'.", "mousemoverel");
            Browser.AddIntellisense("syn.cache_replace(<Instance> obj, <Instance> t_obj)", "Method", "Replace obj in the cache with t_obj.", "syn.cache_replace");
            Browser.AddIntellisense("syn.cache_invalidate(<Instance> obj)", "Method", "Invalidate obj's cache entry, forcing a recache upon the next lookup.", "syn.invalidate_cache");
            Browser.AddIntellisense("syn.open_web_socket(<string> name)", "Method", "Open's the Synapse WebSocket with channel name. This function will not exist if the user did not enable WebSocket support in theme.json.", "syn.open_web_socket");
        }

        public void Attach()
        {
            if (Worker.IsBusy || IsInlineUpdating) return;

            Worker.RunWorkerAsync();
        }

        public void SetEditor(string Text)
        {
            Browser.SetText(Text);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.Theme.Main.WebSocket.Enabled)
            {
                WebSocketInterface.Stop();
            }

            DataInterface.Save("savedws", Browser.GetText());
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (OptionsOpen) return;

            var Options = new OptionsWindow(this);
            Options.Show();
        }

        private void MiniButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Browser.SetText("");
        }

        private void IconBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(
                "Synapse X was developed by 3dsboy08, brack4712, Louka, DefCon42, and Eternal.\r\n\r\nAdditional credits:\r\n    - Rain: Emotional support and testing",
                "Synapse X Credits", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var OpenDialog = new OpenFileDialog
            {
                Filter = "Script Files (*.lua, *.txt)|*.lua;*.txt", Title = "Synapse X - Open File", FileName = ""
            };

            if (OpenDialog.ShowDialog() != true) return;

            try
            {
                Browser.SetText(File.ReadAllText(OpenDialog.FileName));
            }
            catch (Exception ex) { Console.WriteLine(ex); }
        }

        private void ExecuteFileButton_Click(object sender, RoutedEventArgs e)
        {
            var OpenDialog = new OpenFileDialog
            {
                Filter = "Script Files (*.lua, *.txt)|*.lua;*.txt", Title = "Synapse X - Execute File", FileName = ""
            };

            if (OpenDialog.ShowDialog() != true) return;

            try
            {
                Execute(File.ReadAllText(OpenDialog.FileName));
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to read file. Check if it is accessible.", "Synapse X", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            var SaveDialog = new SaveFileDialog {Filter = "Script Files (*.lua, *.txt)|*.lua;*.txt", FileName = ""};

            SaveDialog.FileOk += (o, args) =>
            {
                File.WriteAllText(SaveDialog.FileName, Browser.GetText());
            };

            SaveDialog.ShowDialog();
        }

        private void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            if (Worker.IsBusy) return;

            Worker.RunWorkerAsync();
        }

        private void ScriptHubButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptHubOpen) return;
            if (ScriptHubInit) return;

            ScriptHubOpen = true;
            ScriptHubInit = true;

            ScriptHubButton.Content = Globals.Theme.Main.ScriptHubButton.TextYield;
            HubWorker.RunWorkerAsync();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            Execute(Browser.GetText());
        }

        private void ExecuteItem_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptBox.SelectedIndex == -1) return;

            try
            {
                var Element = ScriptBox.Items[ScriptBox.SelectedIndex].ToString();

                Execute(File.ReadAllText(Path.Combine(ScriptsDirectory, Element)));
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to read file. Check if it is accessible.", "Synapse X", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void LoadItem_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptBox.SelectedIndex == -1) return;

            try
            {
                var Element = ScriptBox.Items[ScriptBox.SelectedIndex].ToString();

                Browser.SetText(File.ReadAllText(Path.Combine(ScriptsDirectory, Element)));
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to read file. Check if it is accessible.", "Synapse X", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void RefreshItem_Click(object sender, RoutedEventArgs e)
        {
            ScriptBox.Items.Clear();

            foreach (var FilePath in Directory.GetFiles(ScriptsDirectory))
            {
                ScriptBox.Items.Add(Path.GetFileName(FilePath));
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void HubWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            WebInterface.VerifyWebsite(this);

            var Data = WebInterface.GetScriptHubData();

            Dispatcher.Invoke(() =>
            {
                ScriptHubInit = false;
                ScriptHubButton.Content = Globals.Theme.Main.ScriptHubButton.TextNormal;

                var ScriptHub = new ScriptHubWindow(this, Data);
                ScriptHub.Show();
            });
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public void InlineAutoUpdate()
        {
            SetTitle(" (not running latest version! updating...)");

            var Data = WebInterface.GetData();

            if (Data.UiVersion != LoadWindow.UiVersion)
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
                SetTitle(" (not currently updated! please wait for an update to release.)", 3000);
                return;
            }
#endif

            var DllName = "bin\\" + Utils.CreateFileName("Synapse.dll");
            var LauncherName = "bin\\" + Utils.CreateFileName("Synapse Launcher.exe");
            const string SxLibName = "bin\\sxlib\\sxlib.dll";
            const string SxLibXmlName = "bin\\sxlib\\sxlib.xml";
            
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
                        SetTitle(" (updating DLLs...)");

                        WC.DownloadFile(Globals.Options.BetaRelease ? Data.BetaDllDownload : Data.DllDownload, DllName);
                        Utils.AppendAllBytes(DllName, Salt);
                    }

                    if (Utils.Sha512Dll(DllName) != (Globals.Options.BetaRelease ? Data.BetaDllHash : Data.DllHash))
                    {
                        SetTitle(" (updating DLLs...)");

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
                        SetTitle(" (updating Monaco...)");

                        WC.DownloadFile("https://cdn.synapse.to/synapsedistro/distro/Monaco.zip", "bin\\Monaco.zip");

                        ZipFile.ExtractToDirectory("bin\\Monaco.zip", "bin");
                        File.Delete("bin\\Monaco.zip");
                    }

                    if (!File.Exists("bin\\CefSharp.dll"))
                    {
                        SetTitle(" (updating CefSharp...)");

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
                        SetTitle(" (updating SQLite...)");

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

                    if (!File.Exists(SxLibName))
                    {
                        SetTitle(" (updating sxlib...)");

                        WC.DownloadFile(Data.SxLibDownload, SxLibName);
                    }

                    if (Utils.Sha512(SxLibName, true) != Data.SxLibHash)
                    {
                        SetTitle(" (updating sxlib...)");

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
                    if (!File.Exists(LauncherName))
                    {
                        SetTitle(" (updating launcher...)");

                        WC.DownloadFile(Data.LauncherDownload, LauncherName);
                    }

                    if (Utils.Sha512(LauncherName, true) != Data.LauncherHash)
                    {
                        SetTitle(" (updating launcher...)");

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
            SetTitle(" (update complete! reinjecting...)");

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

                    Dispatcher.Invoke(() =>
                    {
                        SetTitle(AttachStrings.FailedToFindRoblox, 3000);
                        InteractMessageRecieved?.Invoke(this, "SYN_FAILED_TO_FIND");
                    });

                    return;
                }

                if (ProcList[0].Id == RbxId)
                {
                    IsInlineUpdating = false;

                    Dispatcher.Invoke(() =>
                    {
                        SetTitle(AttachStrings.AlreadyInjected, 3000);
                        InteractMessageRecieved?.Invoke(this, "SYN_ALREADY_INJECTED");
                    });

                    return;
                }

                ProcId = ProcList[0].Id;
            }

            RobloxIdTemp = ProcId;
            CInterface.Inject(BaseDirectory + "\\" + Globals.DllPath,
                BaseDirectory + "\\bin\\redis\\D3DCompiler_43.dll", BaseDirectory + "\\bin\\redis\\xinput1_3.dll",
                ProcId, false);

            IsInlineUpdating = false;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
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
                    Dispatcher.Invoke(() =>
                    {
                        SetTitle(AttachStrings.FailedToFindRoblox, 3000);
                        InteractMessageRecieved?.Invoke(this, "SYN_FAILED_TO_FIND");
                    });

                    return;
                }

                if (ProcList[0].Id == RbxId)
                {
                    Dispatcher.Invoke(() =>
                    {
                        SetTitle(AttachStrings.AlreadyInjected, 3000);
                        InteractMessageRecieved?.Invoke(this, "SYN_ALREADY_INJECTED");
                    });

                    return;
                }

                ProcId = ProcList[0].Id;
            }

            Dispatcher.Invoke(() =>
            {
                SetTitle(AttachStrings.Checking);
            });

            if (Globals.Version != WebInterface.VerifyWebsiteWithVersion(this))
            {
                IsInlineUpdating = true;
                new Thread(InlineAutoUpdate).Start();

                return;
            }

            Dispatcher.Invoke(() =>
            {
                SetTitle(AttachStrings.Injecting);
                InteractMessageRecieved?.Invoke(this, "SYN_INJECTING");
            });

            RobloxIdTemp = ProcId;
            CInterface.Inject(BaseDirectory + "\\" + Globals.DllPath,
                BaseDirectory + "\\bin\\redis\\D3DCompiler_43.dll", BaseDirectory + "\\bin\\redis\\xinput1_3.dll",
                ProcId, false);
        }
    }
}
