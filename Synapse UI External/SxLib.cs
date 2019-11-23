using System;
using System.Windows;
using System.Windows.Forms;
using sxlib.Specialized;

namespace sxlib
{
    /// <summary>
    /// Setup class for SxLib.
    /// </summary>
    public static class SxLib
    {
        /// <summary>
        /// Sets up SxLib for WinForms.
        /// </summary>
        /// <param name="Startup">Your initialization form.</param>
        /// <param name="SynapseDirectory">The base Synapse directory.</param>
        public static SxLibWinForms InitializeWinForms(Form Startup, string SynapseDirectory)
        {
            return new SxLibWinForms(Startup, SynapseDirectory);
        }

        /// <summary>
        /// Sets up SxLib for WPF.
        /// </summary>
        /// <param name="Startup">Your initialization window.</param>
        /// <param name="SynapseDirectory">The base Synapse directory.</param>
        public static SxLibWPF InitializeWPF(Window Startup, string SynapseDirectory)
        {
            return new SxLibWPF(Startup, SynapseDirectory);
        }

        /// <summary>
        /// Sets up SxLib for non-GUI apps.
        /// </summary>
        /// <param name="SynapseDirectory">The base Synapse directory.</param>
        public static SxLibOffscreen InitializeOffscreen(string SynapseDirectory)
        {
            return new SxLibOffscreen(SynapseDirectory);
        }
    }
}
