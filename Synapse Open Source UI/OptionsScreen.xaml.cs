using System;
using System.Threading.Tasks;
using System.Windows;
using sxlib.Static;
using Synapse_X_UI.Properties;

namespace Synapse_X_UI
{
    /// <summary>
    /// Interaction logic for ScriptHubScreen.xaml
    /// </summary>
    public partial class OptionsScreen
    {
        private readonly InterfaceDesign designMethods;
        private readonly Window window;
        private Data.Options oldOptions;
        private bool active;
        private bool launchStatus;
        private bool betaStatus;

        public OptionsScreen(Window curWindow)
        {
            InitializeComponent();
            designMethods = new InterfaceDesign();
            window = curWindow;
            Left = curWindow.Left + 400;
            Top = curWindow.Top;

            Title = Globals.RandomString(Globals.Rnd.Next(10, 32));
        }

        private async void OptionsScreen_OnLoaded(object sender, RoutedEventArgs e)
        {
            var Options = Globals.SxLib.GetOptions();
            oldOptions = Options;

            launchStatus = Options.AutoLaunch;
            betaStatus = Options.BetaRelease;

            topMostCheck.IsChecked = Settings.Default.topmost;
            disableSoundsCheck.IsChecked = Settings.Default.disableSounds;
            unlockFPSCheck.IsChecked = Options.UnlockFPS;
            autoattachCheck.IsChecked = Options.AutoAttach;
            interalUICheck.IsChecked = Options.InternalUI;
            autolaunchCheck.IsChecked = Options.AutoLaunch;
            betareleaseCheck.IsChecked = Options.BetaRelease;

            designMethods.ShiftWindow(optionsScreen, Left, Top, Left + 325, Top);
            await Task.Delay(1000);
            active = true;
            ExploitScreen.debounce = false;
            window.LocationChanged += Window_LocationChanged;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (!active) return;
            Left = window.Left + 725;
            Top = window.Top;
        }

        private async void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Globals.SxLib.SetOptions(new Data.Options
            { 
                AutoAttach = autoattachCheck.IsChecked.Value,
                AutoLaunch = autolaunchCheck.IsChecked.Value,
                IngameChat = oldOptions.IngameChat,
                BetaRelease = betareleaseCheck.IsChecked.Value,
                InternalUI = interalUICheck.IsChecked.Value,
                MultiRoblox = oldOptions.MultiRoblox,
                UnlockFPS = unlockFPSCheck.IsChecked.Value,
                WindowScale = oldOptions.WindowScale
            });

            window.Focus();
            active = false;
            ExploitScreen.debounce = true;
            designMethods.ShiftWindow(optionsScreen, Left, Top, Left - 325, Top);
            await Task.Delay(1000);
            ExploitScreen.debounce = false;
            Close();
        }

        private void topMostHandler(object sender, RoutedEventArgs e)
        {
            Settings.Default.topmost = topMostCheck.IsChecked ?? false;
            Settings.Default.Save();
        }

        private void disableSoundsHandler(object sender, RoutedEventArgs e)
        {
            Settings.Default.disableSounds = disableSoundsCheck.IsChecked ?? false;
            Settings.Default.Save();
        }

        private void autoLaunchHandler(object sender, RoutedEventArgs e)
        {
            if (autolaunchCheck.IsChecked == launchStatus) return;

            var Result = MessageBoxResult.Yes;
            if (autolaunchCheck.IsChecked == true)
            {
                Result = MessageBox.Show(
                    "You have selected to enable the AutoLaunch option for Synapse.\n\nPlease note that this option replaces your launcher with a custom one made by Synapse X. Are you sure you want to continue?",
                    "Synapse X - AutoLaunch", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No);
            }

            if (Result == MessageBoxResult.Yes) return;
            autolaunchCheck.IsChecked = launchStatus;
        }

        private void betaReleaseHandler(object sender, RoutedEventArgs e)
        {
            if (betareleaseCheck.IsChecked == betaStatus) return;

            var Result = MessageBoxResult.Yes;
            if (betareleaseCheck.IsChecked == true)
            {
                Result = MessageBox.Show(
                    "You have selected to enable the beta release program for Synapse.\n\nPlease note you might expirence crashes or other issues that are not present in the release build of Synapse X. Are you sure you want to continue?",
                    "Synapse X - Beta Notification", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No);
            }

            if (Result == MessageBoxResult.Yes) return;
            betareleaseCheck.IsChecked = betaStatus;
        }
    }
}
