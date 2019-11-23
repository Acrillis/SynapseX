using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using sxlib.Internal;
using sxlib.Static;

namespace sxlib.Specialized
{
    /// <summary>
    /// SxLib for WPF-based UIs.
    /// </summary>
    public class SxLibWPF : SxLibBase
    {
        private Window Current;

        /// <summary>
        /// A delegate for a callback from SxLib.Load().
        /// </summary>
        /// <param name="Event">A enum containing states of Synapse X loading.</param>
        /// <param name="Param">An extra parameter used for some events.</param>
        public new delegate void SynLoadDelegate(SynLoadEvents Event, object Param);

        /// <summary>
        /// This event will be periodically called back during Synapse X initialization. Use SxLib.Load() to start initialization.
        /// </summary>
        public event SynLoadDelegate LoadEvent;

        /// <summary>
        /// A delegate for a callback from SxLib.Attach().
        /// </summary>
        /// <param name="Event">A enum containing states of Synapse X attaching.</param>
        /// <param name="Param">An extra parameter used for some events.</param>
        public new delegate void SynAttachDelegate(SynAttachEvents Event, object Param);

        /// <summary>
        /// This event will be periodically called back during Synapse X attaching. Use SxLib.Attach() to start the attaching process.
        /// </summary>
        public event SynAttachDelegate AttachEvent;

        /// <summary>
        /// A delegate for a callback from SxLib.ScriptHub().
        /// </summary>
        /// <param name="Entries">A list of script hub entries.</param>
        public new delegate void SynScriptHubDelegate(List<SynHubEntry> Entries);

        /// <summary>
        /// This event will be called after Synapse X successfully grabs the Script Hub contents from the SxLib.ScriptHub method.
        /// </summary>
        public event SynScriptHubDelegate ScriptHubEvent;

        /// <summary>
        /// Do not call this constructor. Use SxLib.InitializeWPF instead.
        /// </summary>
        /// <param name="_Current"></param>
        /// <param name="_SynapseDir"></param>
        protected internal SxLibWPF(Window _Current, string _SynapseDir) : base(_SynapseDir)
        {
            Current = _Current;
            LoadEventInternal += delegate (SynLoadEvents LEvent, object Param) { Current.Dispatcher.Invoke(() => { LoadEvent?.Invoke(LEvent, Param); }); };
            AttachEventInternal += delegate (SynAttachEvents AEvent, object Param) { Current.Dispatcher.Invoke(() => { AttachEvent?.Invoke(AEvent, Param); }); };
            HubEventInternal += delegate (List<SynHubEntry> Entries) { Current.Dispatcher.Invoke(() => { ScriptHubEvent?.Invoke(Entries); }); };
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        protected override void VerifyWebsite()
        {
            WebInterface.VerifyWebsite(Current);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        protected override string VerifyWebsiteWithVersion()
        {
            return WebInterface.VerifyWebsiteWithVersion(Current);
        }

        /// <summary>
        /// This will start the initial loading process of Synapse X. You must attach a handler to 'LoadEvent' to get callback events from this function.
        /// </summary>
        /// <returns>If initial loading fails, this will return false.</returns>
        public bool Load()
        {
            return LoadInternal();
        }

        /// <summary>
        /// This will start the initial attaching process of Synapse X. You must attach a handler to 'AttachEvent' to get callback events from this function.
        /// </summary>
        /// <returns>If initial attaching fails, this will return false.</returns>
        public bool Attach()
        {
            return AttachInternal();
        }

        /// <summary>
        /// This will start the script hub. You must attach a handler to 'ScriptHubEvent' to get the actual script hub contents from this function.
        /// </summary>
        /// <returns>If the script hub is already open, this will return false. Use SxLib.ScriptHubMarkAsClosed() to allow the script hub to be open again.</returns>
        public bool ScriptHub()
        {
            return ScriptHubInternal();
        }

        /// <summary>
        /// This will return if Synapse X can execute scripts at the current time.
        /// </summary>
        /// <returns>If Synapse X is fully loaded and can execute scripts.</returns>
        public bool Ready()
        {
            return ReadyInternal();
        }

        /// <summary>
        /// This will execute a script in Synapse X. Note if you try to call this function without being attached, you will get a 'NOT_ATTACHED' event from the AttachEvent callback.
        /// </summary>
        /// <param name="Script">The script to be executed.</param>
        public void Execute(string Script)
        {
            ExecuteInternal(Script);
        }

        /// <summary>
        /// This will mark the Script Hub as closed, allowing you to call SxLib.ScriptHub() again.
        /// </summary>
        public void ScriptHubMarkAsClosed()
        {
            ScriptHubMarkAsClosedInternal();
        }

        /// <summary>
        /// This will return the current options the user has selected.
        /// </summary>
        /// <returns>The user's options.</returns>
        public Data.Options GetOptions()
        {
            return GetOptionsInternal();
        }

        /// <summary>
        /// This will set the options of the current user.
        /// </summary>
        /// <param name="Options">The options to set.</param>
        public void SetOptions(Data.Options Options)
        {
            SetOptionsInternal(Options);
        }

        /// <summary>
        /// Set the Window currently in use. You must call this every time you create a new 'main' form.
        /// </summary>
        /// <param name="_Current">The current window.</param>
        public void SetWindow(Window _Current)
        {
            Current = _Current;
        }
    }
}
