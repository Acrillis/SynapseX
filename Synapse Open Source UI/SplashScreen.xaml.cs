using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using sxlib;
using sxlib.Specialized;

namespace Synapse_X_UI
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen
    {
        private readonly InterfaceDesign designMethods;
        private readonly Random rand;
        private bool loading = true;

        public SplashScreen()
        {
            InitializeComponent();
            designMethods = new InterfaceDesign();
            rand = new Random();
        }

        private async void SplashScreen_OnLoaded(object sender, RoutedEventArgs e)
        {
            Title = Globals.RandomString(Globals.Rnd.Next(10, 32));

            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                string hexFrom = "#FFFFFF";
                string hexTo = $"#{rand.Next(0x1000000):X6}";
                while (loading)
                {
                    var @from = hexFrom;
                    var to = hexTo;
                    Dispatcher.Invoke(() => { designMethods.FontColor(loadTextX, @from, to); });
                    hexFrom = hexTo;
                    hexTo = $"#{rand.Next(0x1000000):X6}";
                    await Task.Delay(900);
                }
            }).Start();
            foreach (FrameworkElement element in logos.Children)
            {
                designMethods.FadeIn(element);
            }
            designMethods.Shift(loadLogo, loadLogo.Margin, new Thickness(262, 62, 262, 98));
            designMethods.Shift(loadText, loadText.Margin, new Thickness(270, 248, 302, 60));
            designMethods.Shift(loadTextX, loadTextX.Margin, new Thickness(422, 248, 268, 60));
            designMethods.Shift(statusText, statusText.Margin, new Thickness(0, 255, 0, 20));
            await Task.Delay(500);
            designMethods.Resize(loadEllipse, 300, 300);
            // do all loading here //

            var ProcList = Process.GetProcessesByName(
                Path.GetFileName(AppDomain.CurrentDomain.FriendlyName));
            var Current = Process.GetCurrentProcess();
            foreach (var Proc in ProcList)
            {
                if (Proc.Id == Current.Id) continue;
                try
                {
                    Proc.Kill();
                }
                catch (Exception)
                {
                }
            }

            var SLib = SxLib.InitializeWPF(this, Path.Combine(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName) + "\\");
            SLib.LoadEvent += async (SEvent, Param) =>
            {
                switch (SEvent)
                {
                    case SxLibBase.SynLoadEvents.CHECKING_WL:
                        statusText.Content = "checking whitelist...";
                        break;
                    case SxLibBase.SynLoadEvents.CHANGING_WL:
                        statusText.Content = "changing whitelist...";
                        break;
                    case SxLibBase.SynLoadEvents.DOWNLOADING_DATA:
                        statusText.Content = "downloading data...";
                        break;
                    case SxLibBase.SynLoadEvents.CHECKING_DATA:
                        statusText.Content = "checking data...";
                        break;
                    case SxLibBase.SynLoadEvents.DOWNLOADING_DLLS:
                        statusText.Content = "downloading dlls...";
                        break;

                    case SxLibBase.SynLoadEvents.READY:
                    {
                        statusText.Content = "ready!";

                        await Task.Delay(1000);
                        designMethods.Shift(loadLogo, loadLogo.Margin, new Thickness(262, 42, 262, 118));
                        designMethods.Shift(loadText, loadText.Margin, new Thickness(270, 228, 302, 80));
                        designMethods.Shift(loadTextX, loadTextX.Margin, new Thickness(422, 228, 268, 80));
                        designMethods.Shift(statusText, statusText.Margin, new Thickness(0, 235, 0, 40));
                        foreach (FrameworkElement element in logos.Children)
                        {
                            designMethods.FadeOut(element);
                        }
                        designMethods.Resize(loadEllipse, 1000, 1000);
                        await Task.Delay(900);
                        loading = false;
                        ExploitScreen exploit = new ExploitScreen();
                        exploit.Show();
                        Close();
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(SEvent), SEvent, null);
                }
            };
            SLib.Load();
            Globals.SxLib = SLib;
        }
    }
}
