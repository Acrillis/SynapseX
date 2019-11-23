using System.Windows;

namespace Synapse_X_UI
{
    /// <summary>
    /// Interaction logic for LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Window
    {
        public LoginScreen()
        {
            InitializeComponent();
        }

        private void LoginScreen_OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // to be implemented
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // to be implemented
        private void forgotButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // to be implemented
        private void serialButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
