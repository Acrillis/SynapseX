using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Synapse_UI_WPF.Interfaces;

namespace Synapse_UI_WPF
{
    public partial class ResetWindow
    {
        public BackgroundWorker ResetEmailWorker = new BackgroundWorker();
        public BackgroundWorker ResetPasswordWorker = new BackgroundWorker();

        public ResetWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResetEmailWorker.DoWork += ResetEmailWorker_DoWork;
            ResetPasswordWorker.DoWork += ResetPasswordWorker_DoWork;

            InitializeComponent();
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = WebInterface.RandomString(WebInterface.Rnd.Next(10, 32));
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private bool ResettingEmail;
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private bool ResettingPassword;

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void ResetEmailButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResettingEmail) return;

            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                MessageBox.Show(
                    "You did not enter a username or email!",
                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ResetEmailButton.Content = "Sending Reset Email...";
            ResettingEmail = true;
            ResetEmailWorker.RunWorkerAsync();
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResettingPassword) return;

            if (string.IsNullOrWhiteSpace(ResetTokenBox.Text) || string.IsNullOrWhiteSpace(NewPasswordBox.Password))
            {
                MessageBox.Show(
                    "You did not enter a reset token or new password!",
                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (NewPasswordBox.Password != NewPasswordConfirmBox.Password)
            {
                MessageBox.Show(
                    "Your passwords do not match!",
                    "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ResetPasswordButton.Content = "Restting Password...";
            ResettingPassword = true;
            ResetPasswordWorker.RunWorkerAsync();
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void ResetEmailWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string UsernameEmail = null;

            Dispatcher.Invoke(() => { UsernameEmail = UsernameBox.Text; });

            WebInterface.VerifyWebsite(this);
            var Result = WebInterface.SendResetEmail(UsernameEmail);

            switch (Result)
            {
                case WebInterface.ResetEmailResult.OK:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "A password reset token has been sent to the email you specified when you created your Synapse account.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                        ResetEmailButton.Content = "Send Reset Email";
                        ResettingEmail = false;
                    });
                    break;
                }
                case WebInterface.ResetEmailResult.NOT_ENOUGH_TIME:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "You have submitted another reset request in the last 2 hours, please wait 2 hours and try again.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                    });
                    return;
                }
                case WebInterface.ResetEmailResult.ACCOUNT_DOES_NOT_EXIST:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "You have entered an invalid username or email.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        ResetEmailButton.Content = "Send Reset Email";
                        ResettingEmail = false;
                    });
                    return;
                }
                case WebInterface.ResetEmailResult.INVALID_REQUEST:
                case WebInterface.ResetEmailResult.UNKNOWN:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Failed to send a password reset request to Synapse account. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        ResetEmailButton.Content = "Send Reset Email";
                        ResettingEmail = false;
                    });
                    return;
                }
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void ResetPasswordWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string ResetToken = null, NewPassword = null;

            Dispatcher.Invoke(() =>
            {
                ResetToken = ResetTokenBox.Text;
                NewPassword = NewPasswordBox.Password;
            });

            WebInterface.VerifyWebsite(this);
            var Result = WebInterface.ResetPassword(ResetToken, NewPassword);

            switch (Result.Result)
            {
                case WebInterface.ResetPasswordResult.OK:
                {
                    Dispatcher.Invoke(() =>
                    {
                        Clipboard.SetText("Username: " + Result.Username + "\nPassword: " + NewPassword);
                        MessageBox.Show(
                            "You have successfully reset your password!\nYour whitelist information goes as follows:\n\nUsername: " + Result.Username + "\nPassword: " + NewPassword + "\n\nThis information has been copied to your clipboard. Keep it in a safe place.\nYou can now restart Synapse and enter your new password.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
                        Environment.Exit(0);
                    });
                    break;
                }
                case WebInterface.ResetPasswordResult.INVALID_KEY:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "You have entered an invalid reset token.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        ResetPasswordButton.Content = "Reset Password";
                        ResettingPassword = false;
                    });
                    return;
                }
                case WebInterface.ResetPasswordResult.KEY_EXPIRED:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Your reset token has expired. Please send a new token and try again.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        ResetPasswordButton.Content = "Reset Password";
                        ResettingPassword = false;
                    });
                    return;
                }
                case WebInterface.ResetPasswordResult.ACCOUNT_DOES_NOT_EXIST:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "The account attempting to be reset does not exist. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        ResetPasswordButton.Content = "Reset Password";
                        ResettingPassword = false;
                    });
                    return;
                }
                case WebInterface.ResetPasswordResult.INVALID_REQUEST:
                case WebInterface.ResetPasswordResult.UNKNOWN:
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Failed to reset password to Synapse account. Please contact 3dsboy08 on Discord.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        ResetPasswordButton.Content = "Reset Password";
                        ResettingPassword = false;
                    });
                    return;
                }
            }
        }
    }
}
