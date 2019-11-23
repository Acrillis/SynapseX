using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json;
using Synapse_UI_WPF.Interfaces;
using Synapse_UI_WPF.Static;
using Path = System.IO.Path;

namespace Synapse_UI_WPF
{
    public partial class OptionsWindow
    {
        private readonly MainWindow Main;
        private bool BetaStatus;
        private bool LaunchStatus;

        public OptionsWindow(MainWindow _Main)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeComponent();

            Main = _Main;
            Main.OptionsOpen = true;
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

            BetaStatus = Globals.Options.BetaRelease;
            LaunchStatus = Globals.Options.AutoLaunch;

            AutoLaunchBox.IsChecked = Globals.Options.AutoLaunch;
            AutoAttachBox.IsChecked = Globals.Options.AutoAttach;
            UnlockBox.IsChecked = Globals.Options.UnlockFPS;
            InternalUIBox.IsChecked = Globals.Options.InternalUI;
            IngameChatBox.IsChecked = Globals.Options.IngameChat;
            BetaReleaseBox.IsChecked = Globals.Options.BetaRelease;
            ScaleSlider.Value = Globals.Options.WindowScale;

            ScaleSetup = true;
        }

        private void BetaReleaseBox_Click(object sender, RoutedEventArgs e)
        {
            if (BetaReleaseBox.IsChecked == BetaStatus) return;

            var Result = MessageBoxResult.Yes;
            if (BetaReleaseBox.IsChecked == true)
            {
                Result = MessageBox.Show(
                    "You have selected to enable the beta release program for Synapse.\n\nPlease note you might expirence crashes or other issues that are not present in the release build of Synapse X. Are you sure you want to continue?",
                    "Synapse X - Beta Notification", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No);
            }

            if (Result == MessageBoxResult.Yes) return;
            BetaReleaseBox.IsChecked = BetaStatus;
        }

        private void AutoLaunchBox_Click(object sender, RoutedEventArgs e)
        {
            if (AutoLaunchBox.IsChecked == LaunchStatus) return;

            var Result = MessageBoxResult.Yes;
            if (AutoLaunchBox.IsChecked == true)
            {
                Result = MessageBox.Show(
                    "You have selected to enable the AutoLaunch option for Synapse.\n\nPlease note that this option replaces your launcher with a custom one made by Synapse X. Are you sure you want to continue?",
                    "Synapse X - AutoLaunch", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No);
            }

            if (Result == MessageBoxResult.Yes) return;
            AutoLaunchBox.IsChecked = LaunchStatus;
        }

        private bool ScaleDebounce;
        private bool ScaleSetup;

        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!ScaleSetup || ScaleDebounce || e == null) return;

            var NewValue = e.NewValue;

            if (Math.Abs(NewValue) <= 0)
            {
                ScaleDebounce = true;
                ScaleSlider.Value = 0.1;
                ScaleDebounce = false;

                MessageBox.Show("Scale cannot be 0.", "Synapse X", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Main.ScaleTransform.ScaleX = NewValue;
            Main.ScaleTransform.ScaleY = NewValue;
        }
        private void ResetLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ScaleSlider.Value = 1.0d;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.Options = new Data.Options
            {
                AutoAttach = AutoAttachBox.IsChecked.Value,
                AutoLaunch = AutoLaunchBox.IsChecked.Value,
                MultiRoblox = false,
                UnlockFPS = UnlockBox.IsChecked.Value,
                InternalUI = InternalUIBox.IsChecked.Value,
                IngameChat = IngameChatBox.IsChecked.Value,
                BetaRelease = BetaReleaseBox.IsChecked.Value,
                WindowScale = ScaleSlider.Value
            };
            DataInterface.Save("options", new Data.OptionsHolder
            {
                Version = Data.OptionsVersion,
                Data = JsonConvert.SerializeObject(Globals.Options)
            });

            if (BetaStatus != BetaReleaseBox.IsChecked)
            {
                MessageBox.Show(
                    "You have chosen to either enable/disable Synapse X beta releases. You must now restart Synapse X in order to install the beta release.",
                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(0);
            }

            if (LaunchStatus != AutoLaunchBox.IsChecked)
            {
                try
                {
                    var Key = Registry.ClassesRoot.OpenSubKey("roblox-player\\shell\\open\\command", true);
                    var BaseDirectory = Path.Combine(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName);
                    if (Key == null) throw new Exception("SubKey is invalid.");

                    if (AutoLaunchBox.IsChecked == true)
                    {
                        var Value = ((string) Key.GetValue("")).Split('"').Where((Item, Idx) => Idx % 2 != 0).ToArray()[0];
                        Key.SetValue("", $"\"{BaseDirectory + "\\" + Globals.LauncherPath}\" %1");
                        DataInterface.Save("launcherbackup", Value);
                    }
                    else
                    {
                        if (!DataInterface.Exists("launcherbackup"))
                        {
                            MessageBox.Show("Failed to get launcher backup. You should reinstall Roblox.", "Synapse X",
                                MessageBoxButton.OK, MessageBoxImage.Warning);

                            Main.OptionsOpen = false;
                            Close();
                            return;
                        }

                        Key.SetValue("", $"\"{DataInterface.Read<string>("launcherbackup")}\" %1");
                        DataInterface.Delete("launcherbackup");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show($"Failed to {((bool)AutoLaunchBox.IsChecked ? "setup" : "remove")} AutoLaunch. Please check your anti-virus software.", "Synapse X",
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }
            }

            if (Main.Ready())
            {
                MessageBox.Show("Some options may not apply until you reinject Synapse.", "Synapse X",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            Main.OptionsOpen = false;
            Main.ScaleTransform.ScaleX = ScaleSlider.Value;
            Main.ScaleTransform.ScaleY = ScaleSlider.Value;

            Close();
        }
    }
}
