using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Synapse_UI_WPF.Interfaces;
using Synapse_UI_WPF.Static;

namespace Synapse_UI_WPF
{
    public partial class RegisterWindow
    {
        public BackgroundWorker RegisterWorker = new BackgroundWorker();

        public RegisterWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            RegisterWorker.DoWork += RegisterWorker_DoWork;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = WebInterface.RandomString(WebInterface.Rnd.Next(10, 32));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
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

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private bool Registering;

        [Obfuscation(Feature = "virtualization", Exclude = false)]

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (Registering) return;

            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                MessageBox.Show(
                    "You did not enter a username!",
                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!UsernameBox.Text.All(char.IsLetterOrDigit))
            {
                MessageBox.Show(
                    "Username is not alphanumeric!",
                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show(
                    "You did not enter a password!",
                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                MessageBox.Show(
                    "You did not enter an email!",
                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(SerialKeyBox.Text))
            {
                MessageBox.Show(
                    "You did not enter a serial key!",
                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            RegisterButton.Content = "Registering...";
            Registering = true;
            RegisterWorker.RunWorkerAsync();
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void RegisterWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string Username = null, Password = null, Email = null, SerialKey = null;

            Dispatcher.Invoke(() =>
            {
                Username = UsernameBox.Text;
                Password = PasswordBox.Password;
                Email = EmailBox.Text;
                SerialKey = SerialKeyBox.Text;
            });

            WebInterface.VerifyWebsite(this);

            var Result = WebInterface.Register(Username, Password, Email, SerialKey);

            switch (Result.Result)
            {
                case WebInterface.RegisterResult.OK:
                {
                    DataInterface.Save("token", Result.Token);
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "You have succesfully created your Synapse account!\n\nYou can now restart Synapse X to use the software.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                    Environment.Exit(0);
                    break;
                }
                case WebInterface.RegisterResult.ALPHA_NUMERIC_ONLY:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Non-alphanumeric usernames are not supported. Please enter an alphanumeric username and try again.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        RegisterButton.Content = "Register";
                        Registering = false;
                    });
                    return;
                }
                case WebInterface.RegisterResult.USERNAME_TAKEN:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Username is taken, please enter a new one and try again.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        RegisterButton.Content = "Register";
                        Registering = false;
                    });
                    return;
                }
                case WebInterface.RegisterResult.ALREADY_EXISTING_ACCOUNT:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "You already have a Synapse account, please use the account you already crated.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        RegisterButton.Content = "Register";
                        Registering = false;
                    });
                    return;
                }
                case WebInterface.RegisterResult.INVALID_KEY:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Invalid serial key.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        RegisterButton.Content = "Register";
                        Registering = false;
                    });
                    return;
                }
                case WebInterface.RegisterResult.INVALID_REQUEST:
                case WebInterface.RegisterResult.UNKNOWN:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Failed to create Synapse account. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        RegisterButton.Content = "Register";
                        Registering = false;
                    });
                    return;
                }
            }
        }
    }
}
