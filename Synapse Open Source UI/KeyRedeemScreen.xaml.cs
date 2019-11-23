using System.Windows;

namespace Synapse_X_UI
{
    /// <summary>
    /// Interaction logic for KeyRedeemScreen.xaml
    /// </summary>
    public partial class KeyRedeemScreen : Window
    {
        public KeyRedeemScreen()
        {
            InitializeComponent();
        }

        private void KeyRedeemScreen_OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
