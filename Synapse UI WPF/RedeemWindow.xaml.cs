using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Synapse_UI_WPF.Interfaces;

namespace Synapse_UI_WPF
{
    public partial class RedeemWindow
    {
        public BackgroundWorker RedeemWorker = new BackgroundWorker();

        public RedeemWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            RedeemWorker.DoWork += RedeemWorker_DoWork;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = WebInterface.RandomString(WebInterface.Rnd.Next(10, 32));

            var Resp = WebInterface.GetUsername();
            if (Resp.Result == WebInterface.GetUsernameResult.OK)
            {
                UsernameBox.Text = Resp.Username;
            }
            else
            {
                switch (Resp.Result)
                {
                    case WebInterface.GetUsernameResult.INVALID_HWID:
                    {
                        MessageBox.Show(
                            "Synapse tried to get your username from the database, but failed. (IH) This should not happen, contact 3dsboy08 on Discord.",
                            "Synapse Xen", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                        return;
                    }
                    case WebInterface.GetUsernameResult.INVALID_REQUEST:
                    {
                        MessageBox.Show(
                            "Synapse tried to get your username from the database, but failed. (IR) This should not happen, contact 3dsboy08 on Discord.",
                            "Synapse Xen", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                        return;
                    }
                    default:
                    {
                        MessageBox.Show(
                            "Synapse tried to get your username from the database, but failed. (" + Convert.ToInt32(Resp.Result) + ") This should not happen, contact 3dsboy08 on Discord.",
                            "Synapse Xen", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                        return;
                    }
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void MiniButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void TextBlock_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://x.synapse.to");
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void RedeemButton_Click(object sender, RoutedEventArgs e)
        {
            if (RedeemWorker.IsBusy) return;
            if (string.IsNullOrEmpty(SerialKeyBox.Text))
            {
                MessageBox.Show(
                    "You did not enter a serial key!",
                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            RedeemButton.Content = "Redeeming...";
            RedeemWorker.RunWorkerAsync();
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void RedeemWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string Username = null, SerialKey = null;

            Dispatcher.Invoke(() =>
            {
                Username = UsernameBox.Text;
                SerialKey = SerialKeyBox.Text;
            });

            WebInterface.VerifyWebsite(this);

            var Result = WebInterface.Redeem(Username, SerialKey);

            switch (Result)
            {
                case WebInterface.RedeemResult.OK:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "You have succesfully redeemed a key to your Synapse account!\n\nYou can now restart Synapse X and login to use the software.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                    Environment.Exit(0);
                    break;
                }
                case WebInterface.RedeemResult.ALREADY_UNLIMITED:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Your account is already unlimited. This should not happen, please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        RedeemButton.Content = "Redeem";
                    });
                    return;
                }
                case WebInterface.RedeemResult.INVALID_KEY:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Invalid serial key.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        RedeemButton.Content = "Redeem";
                    });
                    break;
                }
                case WebInterface.RedeemResult.INVALID_USERNAME:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Invalid username. This should not happen, please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        RedeemButton.Content = "Redeem";
                    });
                    break;
                }
                default:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Failed to redeem to account. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        RedeemButton.Content = "Redeem";
                    });
                    break;
                }
            }
        }
    }
}
