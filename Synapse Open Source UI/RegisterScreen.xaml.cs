using System.Windows;

namespace Synapse_X_UI
{
    /// <summary>
    /// Interaction logic for RegisterScreen.xaml
    /// </summary>
    public partial class RegisterScreen : Window
    {
        public RegisterScreen()
        {
            InitializeComponent();
        }

        private void RegisterScreen_OnLoaded(object sender, RoutedEventArgs e)
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
    }
}
