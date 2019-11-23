using System;
using System.Windows;

namespace Synapse_X_UI
{
    /// <summary>
    /// Interaction logic for PasswordResetScreen.xaml
    /// </summary>
    public partial class PasswordResetScreen : Window
    {
        public PasswordResetScreen()
        {
            InitializeComponent();
        }

        private void PasswordResetScreen_OnLoaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        // to be implemented
        private void sendButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // to be implemented
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
