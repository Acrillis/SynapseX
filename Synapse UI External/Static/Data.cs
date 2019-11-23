using System;
using System.Collections.Generic;

namespace sxlib.Static
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

        /// <summary>
        /// The Synapse X options.
        /// </summary>
        [Serializable]
        public class Options
        {
            /// <summary>
            /// If FPS should be unlocked.
            /// </summary>
            public bool UnlockFPS;

            /// <summary>
            /// If AutoAttach should be enabled.
            /// </summary>
            public bool AutoAttach;

            /// <summary>
            /// If AutoLaunch should be enabled.
            /// </summary>
            public bool AutoLaunch;

            /// <summary>
            /// Currently unused.
            /// </summary>
            public bool MultiRoblox;

            /// <summary>
            /// If the Internal UI should be visible.
            /// </summary>
            public bool InternalUI;

            /// <summary>
            /// If the Ingame Chat should be enabled.
            /// </summary>
            public bool IngameChat;

            /// <summary>
            /// If beta release should be enabled.
            /// </summary>
            public bool BetaRelease;

            /// <summary>
            /// The window scale of the Synapse X UI. You must implement this yourself.
            /// </summary>
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
