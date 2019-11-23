using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Synapse_UI_WPF.Interfaces;
using Synapse_UI_WPF.Static;

namespace Synapse_UI_WPF
{
    public partial class ScriptHubWindow
    {
        private readonly MainWindow Main;

        private readonly Dictionary<string, Data.ScriptHubEntry> DictData = new Dictionary<string, Data.ScriptHubEntry>();

        private Data.ScriptHubEntry CurrentEntry;
        private Data.ScriptHubHolder Data;

        private bool IsExecuting;

        private BackgroundWorker ExecuteWorker = new BackgroundWorker();

        public ScriptHubWindow(MainWindow _Main, Data.ScriptHubHolder _Data)
        {
            Main = _Main;
            Data = _Data;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ExecuteWorker.DoWork += ExecuteWorker_DoWork;

            InitializeComponent();
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

            var TScriptHub = Globals.Theme.ScriptHub;
            ThemeInterface.ApplyWindow(this, TScriptHub.Base);
            ThemeInterface.ApplyLogo(IconBox, TScriptHub.Logo);
            ThemeInterface.ApplySeperator(TopBox, TScriptHub.TopBox);
            ThemeInterface.ApplyLabel(TitleBox, TScriptHub.TitleBox);
            ThemeInterface.ApplyListBox(ScriptBox, TScriptHub.ScriptBox);
            ThemeInterface.ApplyTextBox(DescriptionBox, TScriptHub.DescriptionBox);
            ThemeInterface.ApplyButton(MiniButton, TScriptHub.MinimizeButton);
            ThemeInterface.ApplyButton(ExecuteButton, TScriptHub.ExecuteButton);
            ThemeInterface.ApplyButton(CloseButton, TScriptHub.CloseButton);

            foreach (var Script in Data.Entries)
            {
                DictData[Script.Name] = Script;
                ScriptBox.Items.Add(Script.Name);
            }
        }

        private void ScriptBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScriptBox.SelectedIndex == -1)
            {
                return;
            }

            CurrentEntry = DictData[ScriptBox.Items[ScriptBox.SelectedIndex].ToString()];
            DescriptionBox.Text = CurrentEntry.Description;

            ScriptPictureBox.Source = new BitmapImage(new Uri(CurrentEntry.Picture));
        }

        public bool IsOpen()
        {
            return Dispatcher.Invoke(() =>
            {
                return Application.Current.Windows.Cast<Window>().Any(x => x == this);
            });
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsExecuting) return;
            if (CurrentEntry == null) return;

            if (!Main.Ready())
            {
                ExecuteButton.Content = "Not attached!";

                new Thread(() =>
                {
                    Thread.Sleep(1500);
                    if (!IsOpen()) return;

                    Dispatcher.Invoke(() =>
                    {
                        ExecuteButton.Content = Globals.Theme.ScriptHub.ExecuteButton.TextNormal;
                    });
                }).Start();

                return;
            }

            ExecuteButton.Content = Globals.Theme.ScriptHub.ExecuteButton.TextYield;
            IsExecuting = true;

            ExecuteWorker.RunWorkerAsync();
        }

        private void ExecuteWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string ScriptContent;

            try
            {
                using (var WC = new WebClient())
                {
                    ScriptContent = WC.DownloadString(CurrentEntry.Url);
                }
            }
            catch (Exception)
            {
                if (!IsOpen()) return;

                Dispatcher.Invoke(() =>
                {
                    IsExecuting = false;
                    ExecuteButton.Content = Globals.Theme.ScriptHub.ExecuteButton.TextNormal;

                    Topmost = false;
                    MessageBox.Show(
                        "Synapse failed to download script from the script hub. Check your internet connection.",
                        "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    Topmost = true;
                });

                return;
            }

            Dispatcher.Invoke(() =>
            {
                if (!IsOpen()) return;

                IsExecuting = false;
                ExecuteButton.Content = Globals.Theme.ScriptHub.ExecuteButton.TextNormal;

                Main.Execute(ScriptContent);
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MiniButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Main.ScriptHubOpen = false;
        }
    }
}
