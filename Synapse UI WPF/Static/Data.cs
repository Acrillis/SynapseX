using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse_UI_WPF.Static
{
    public static class Data
    {
        public const uint OptionsVersion = 7;

        [Serializable]
        public class Login
        {
            public string Username;
            public string Password;
        }

        [Serializable]
        public class Options
        {
            public bool UnlockFPS;
            public bool AutoAttach;
            public bool AutoLaunch;
            public bool MultiRoblox;
            public bool InternalUI;
            public bool IngameChat;
            public bool BetaRelease;
            public double WindowScale;
        }

        [Serializable]
        public class OptionsHolder
        {
            public uint Version;
            public string Data;
        }

        [Serializable]
        public class SavedPid
        {
            public int Pid;
            public DateTime StartTime;
        }

        [Serializable]
        public class Discord
        {
            public string Token;
            public string Invite;
        }

        [Serializable]
        public class UIData
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
        public class ScriptHubEntry
        {
            public string Name;
            public string Description;
            public string Picture;
            public string Url;
        }

        [Serializable]
        public class ScriptHubHolder
        {
            public List<ScriptHubEntry> Entries;
        }

        [Serializable]
        public class WebSocketEntry
        {
            public string Origin;
            public string AppName;
            public string DevName;
        }

        [Serializable]
        public class WebSocketHolder
        {
            public List<string> EntriesNoPrompt;
            public List<WebSocketEntry> EntriesPrompt;
        }

        [Serializable]
        public class WebSocketTrustCache
        {
            public List<string> Entries;
        }

        [Serializable]
        public class VerifiedContents<T>
        {
            public T Contents;
            public string Signature;
        }
    }
}
