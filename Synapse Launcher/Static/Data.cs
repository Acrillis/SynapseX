using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse_Launcher.Static
{
    public static class Data
    {
        [Serializable]
        public class SavedPid
        {
            public int Pid;
            public DateTime StartTime;
        }

        [Serializable]
        public class Options
        {
            public bool UnlockFPS;
            public bool AutoAttach;
            public bool MultiRoblox;
            public bool InternalUI;
            public bool IngameChat;
            public bool BetaRelease;
        }

        [Serializable]
        public class OptionsHolder
        {
            public uint Version;
            public string Data;
        }
    }
}
