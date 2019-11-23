using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;

namespace Synapse_UI_WPF.Interfaces
{
    public static class SecurityInterface
    {
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static Tuple<bool, string> ScanMachine()
        {
            var BlProc = new List<string>
            {
                "MicrosoftSoundService",
                "MicrosoftAudioDriver",
                "WindowsAudioDriver",
                "RegAsm",
                "IntelService"
            };

            foreach (var Entry in BlProc)
            {
                if (Process.GetProcessesByName(Entry).Length != 0)
                {
                    return new Tuple<bool, string>(true, "C1-" + Entry[0] + "" + Entry[Entry.Length - 1]);
                }
            }

            var AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            foreach (var F in Directory.GetFiles(AppData))
            {
                try
                {
                    var Mod = ModuleDefMD.Load(F);
                    if (!Mod.GetTypes().Any(Type => Type.Namespace.Contains("Orcus") || Type.Name.Contains("Orcus")))
                        continue;
                    var FileName = Path.GetFileNameWithoutExtension(F);
                    return new Tuple<bool, string>(true, "C2-" + FileName[0] + "" + FileName[FileName.Length - 1]);
                }
                catch (Exception) {}
            }

            var BlAppData = new List<string>	
            {
                "RobIox",
                "Syslog",
                "IntelService"
            };

            foreach (var Entry in BlAppData)
            {
                if (Directory.Exists(Path.Combine(AppData, Entry)))
                {
                    return new Tuple<bool, string>(true, "C3-" + Entry[0] + "" + Entry[Entry.Length - 1]);
                }
            }

            return new Tuple<bool, string>(false, "");
        }
    }
}
