using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using sxlib.Specialized;

namespace Synapse_X_UI
{
    /// <summary>
    /// Interaction logic for ScriptHubScreen.xaml
    /// </summary>
    public partial class ScriptHubScreen
    {
        private readonly InterfaceDesign designMethods;
        private readonly Window window;
        private SxLibBase.SynHubEntry currentEntry;
        private readonly Dictionary<string, SxLibBase.SynHubEntry> hubData = new Dictionary<string, SxLibBase.SynHubEntry>();
        private bool active;

        public ScriptHubScreen(Window curWindow, List<SxLibBase.SynHubEntry> entries)
        {
            InitializeComponent();
            designMethods = new InterfaceDesign();
            window = curWindow;
            Left = curWindow.Left + 400;
            Top = curWindow.Top;

            foreach (var Script in entries)
            {
                hubData[Script.Name] = Script;
                synScripts.Items.Add(Script.Name);
            }

            Title = Globals.RandomString(Globals.Rnd.Next(10, 32));
        }

        private async void ScriptHubScreen_OnLoaded(object sender, RoutedEventArgs e)
        {
            designMethods.ShiftWindow(scriptHubScreen, Left, Top, Left + 325, Top);
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
            Globals.SxLib.ScriptHubMarkAsClosed();
            window.Focus();
            active = false;
            ExploitScreen.debounce = true;
            designMethods.ShiftWindow(scriptHubScreen, Left, Top, Left - 325, Top);
            await Task.Delay(1000);
            ExploitScreen.debounce = false;
            Close();
        }

        public bool IsOpen()
        {
            return Dispatcher.Invoke(() =>
            {
                return Application.Current.Windows.Cast<Window>().Any(x => x == this);
            });
        }

        private void SynScripts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (synScripts.SelectedIndex == -1)
            {
                return;
            }

            currentEntry = hubData[synScripts.Items[synScripts.SelectedIndex].ToString()];
            description.Text = currentEntry.Description;

            thumbnail.Source = new BitmapImage(new Uri(currentEntry.Picture));
        }

        private void ExecuteButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (currentEntry == null) return;

            if (!Globals.SxLib.Ready())
            {
                executeButton.Content = "Not attached!";

                new Thread(() =>
                {
                    Thread.Sleep(1500);
                    if (!IsOpen()) return;

                    Dispatcher.Invoke(() =>
                    {
                        executeButton.Content = "Execute";
                    });
                }).Start();

                return;
            }

            currentEntry.Execute();
        }
    }
}
