using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Synapse_UI_WPF.Interfaces;
using Synapse_UI_WPF.Static;

namespace Synapse_UI_WPF
{
    public partial class LoginWindow
    {
        public BackgroundWorker LoginWorker = new BackgroundWorker();
        private bool FakeClose;

        public LoginWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            LoginWorker.DoWork += LoginWorker_DoWork;

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
            if (FakeClose) return;

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

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FakeClose = true;

            var Register = new RegisterWindow();
            Register.Show();
            Close();
        }
        private void TextBlock_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            FakeClose = true;

            var Reset = new ResetWindow();
            Reset.Show();
            Close();
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoggingIn) return;

            if (string.IsNullOrWhiteSpace(UsernameBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show(
                    "You did not enter a username or password!",
                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LoginButton.Content = "Logging in...";
            LoggingIn = true;
            LoginWorker.RunWorkerAsync();
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private bool LoggingIn;

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void Migrate(string Username, string Password)
        {
            WebInterface.VerifyWebsite(this);
            var Result = WebInterface.Migrate(Username, Password);

            switch (Result.Result)
            {
                case WebInterface.MigrationResult.OK:
                {
                    DataInterface.Save("token", Result.Token);
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "You have successfully migrated your Synapse account!\n\nYou can now restart Synapse X to use the software.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                    Environment.Exit(0);
                    return;
                }
                case WebInterface.MigrationResult.ALREADY_EXISTING_ACCOUNT:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "You already have a Synapse account! Please log into the account you have already created.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoginButton.Content = "Login";
                        LoggingIn = false;
                    });
                    return;
                }
                case WebInterface.MigrationResult.INVALID_USER_PASS:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Invalid username/email or password.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoginButton.Content = "Login";
                        LoggingIn = false;
                    });
                    return;
                }
                case WebInterface.MigrationResult.INVALID_REQUEST:
                case WebInterface.MigrationResult.UNKNOWN:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Failed to migrate Synapse account. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoginButton.Content = "Login";
                        LoggingIn = false;
                    });
                    return;
                }
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private bool Login(string Username, string Password)
        {
            var Result = WebInterface.Login(Username, Password);

            switch (Result.Result)
            {
                case WebInterface.LoginResult.OK:
                {
                    DataInterface.Delete("login");
                    DataInterface.Save("token", Result.Token);
                    return true;
                }
                case WebInterface.LoginResult.INVALID_USER_PASS:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Invalid username/email or password.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoginButton.Content = "Login";
                        LoggingIn = false;
                    });
                    return false;
                }
                case WebInterface.LoginResult.NOT_MIGRATED:
                {
                    Migrate(Username, Password);
                    return false;
                }
                case WebInterface.LoginResult.INVALID_REQUEST:
                case WebInterface.LoginResult.UNKNOWN:
                {
                    Dispatcher.Invoke(() =>
                    {
                        Topmost = false;
                        MessageBox.Show(
                            "Failed to login to Synapse account. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    Environment.Exit(0);
                    return false; 
                }
            }

            return false;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void LoginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string Username = null, Password = null;

            Dispatcher.Invoke(() =>
            {
                Username = UsernameBox.Text;
                Password = PasswordBox.Password;
            });

            WebInterface.VerifyWebsite(this);

            if (!Login(Username, Password)) return;
            var Result = WebInterface.Change(DataInterface.Read<string>("token"));

            switch (Result)
            {
                case WebInterface.ChangeResult.OK:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "You have successfully logged into your Synapse account!\n\nYou can now restart Synapse X to use the software.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                    Environment.Exit(0);
                    return;
                }
                case WebInterface.ChangeResult.INVALID_TOKEN:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Invalid token. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoginButton.Content = "Login";
                        LoggingIn = false;
                    });
                    return;
                }
                case WebInterface.ChangeResult.EXPIRED_TOKEN:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Expired token. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoginButton.Content = "Login";
                        LoggingIn = false;
                    });
                    return;
                }
                case WebInterface.ChangeResult.ALREADY_EXISTING_HWID:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "You already have a Synapse account! Please log into the account you have already created.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoginButton.Content = "Login";
                        LoggingIn = false;
                    });
                    return;
                }
                case WebInterface.ChangeResult.NOT_ENOUGH_TIME:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "You have changed your whitelist too recently. Please wait 24 hours from your last whitelist change and try again.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                    });
                    return;
                }
                case WebInterface.ChangeResult.INVALID_REQUEST:
                case WebInterface.ChangeResult.UNKNOWN:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Failed to change whitelist to Synapse account. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoginButton.Content = "Login";
                        LoggingIn = false;
                    });
                    return;
                }
            }
        }
    }
}
