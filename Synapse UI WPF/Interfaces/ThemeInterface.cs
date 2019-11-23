using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Image = System.Drawing.Image;

namespace Synapse_UI_WPF.Interfaces
{
    public static class ThemeInterface
    {
        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TImage
        {
            public string Path;
            public bool Online;

            public TImage()
            {
                Path = "";
                Online = false;
            }
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TFont
        {
            public string Name;
            public float Size;

            public TFont() { }

            public TFont(string _Name, float _Size)
            {
                Name = _Name;
                Size = _Size;
            }
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TColor
        {
            public byte A;
            public byte R;
            public byte G;
            public byte B;

            public TColor()
            {
                A = 0;
                R = 0;
                G = 0;
                B = 0;
            }

            public TColor(byte _A, byte _R, byte _G, byte _B)
            {
                A = _A;
                R = _R;
                G = _G;
                B = _B;
            }
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TButton
        {
            public TImage Image;
            public TFont Font;
            public TColor BackColor;
            public TColor TextColor;
            public string Text;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TGlyphButton
        {
            public TColor BackColor;
            public TColor GlyphColor;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TYieldButton
        {
            public TImage Image;
            public TFont Font;
            public TColor BackColor;
            public TColor TextColor;
            public string TextNormal;
            public string TextYield;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TListBox
        {
            public TFont Font;
            public TColor BackColor;
            public TColor TextColor;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TLogo
        {
            public TImage Image;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TSeperator
        {
            public bool Enabled;
            public TColor BackColor;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TForm
        {
            public TImage Image;
            public TColor BackColor;
            public bool TopMost;
            public double Opacity;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TWebSocket
        {
            public bool Enabled;
            public bool DebugMode;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TLabel
        {
            public bool Enabled;
            public TFont Font;
            public string Text;
            public TColor BackColor;
            public TColor TextColor;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TFormatLabel
        {
            public bool Enabled;
            public TFont Font;
            public string FormatString;
            public TColor BackColor;
            public TColor TextColor;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TAttachStrings
        {
            public string Checking;
            public string Injecting;
            public string CheckingWhitelist;
            public string Scanning;
            public string Ready;
            public string FailedToFindRoblox;
            public string NotRunningLatestVersion;
            public string NotInjected;
            public string AlreadyInjected;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TInitStrings
        {
            public string CheckingWhitelist;
            public string ChangingWhitelist;
            public string DownloadingData;
            public string CheckingData;
            public string DownloadingDlls;
            public string DownloadingMonaco;
            public string DownloadingCefSharp;
            public string DownloadingSQLite;
            public string JoiningDiscord;
            public string Ready;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TRightClickStrings
        {
            public string Execute;
            public string LoadToEditor;
            public string Refresh;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TEditor
        {
            public bool Light;
            public TColor FixPixel;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TMain
        {
            public TForm Base;
            public TWebSocket WebSocket;
            public TEditor Editor;
            public TAttachStrings BaseStrings;
            public TRightClickStrings RightClickStrings;
            public TLogo Logo;
            public TSeperator TopBox;
            public TFormatLabel TitleBox;
            public TListBox ScriptBox;
            public TGlyphButton MinimizeButton;
            public TGlyphButton ExitButton;
            public TButton ExecuteButton;
            public TButton ClearButton;
            public TButton OpenFileButton;
            public TButton ExecuteFileButton;
            public TButton SaveFileButton;
            public TButton OptionsButton;
            public TButton AttachButton;
            public TYieldButton ScriptHubButton;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TScriptHub
        {
            public TForm Base;
            public TLogo Logo;
            public TSeperator TopBox;
            public TLabel TitleBox;
            public TListBox ScriptBox;
            public TListBox DescriptionBox;
            public TGlyphButton MinimizeButton;
            public TYieldButton ExecuteButton;
            public TButton CloseButton;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TLoad
        {
            public TForm Base;
            public TInitStrings BaseStrings;
            public TLogo Logo;
            public TSeperator TopBox;
            public TLabel TitleBox;
            public TLabel StatusBox;
        }

        [Serializable]
        [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = true)]
        public class TBase
        {
            public uint Version;
            public TLoad Load;
            public TMain Main;
            public TScriptHub ScriptHub;
        }

        public static SolidColorBrush ConvertColor(TColor ThemeColor)
        {
            return new SolidColorBrush(Color.FromArgb(ThemeColor.A, ThemeColor.R, ThemeColor.G, ThemeColor.B));
        }

        public static string ConvertFormatString(TFormatLabel ThemeLabel, string Version)
        {
            return ThemeLabel.FormatString.Replace("{version}", Version);
        }

        public static BitmapImage ConvertImage(TImage ThemeImage)
        {
            try
            {
                if (ThemeImage.Path == "") return null;
                if (!ThemeImage.Online) return new BitmapImage(new Uri(ThemeImage.Path));
                using (var WC = new WebClient())
                {
                    var Data = WC.DownloadData(ThemeImage.Path);

                    using (var Stream = new MemoryStream(Data))
                    {
                        var Bitmap = new BitmapImage();
                        Bitmap.BeginInit();
                        Bitmap.StreamSource = Stream;
                        Bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        Bitmap.EndInit();
                        Bitmap.Freeze();

                        return Bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to parse image.\n\nException details:\n" + ex.Message,
                    "Synapse X Image Parser", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static void ApplyButton(Button Button, TButton ThemeButton)
        {
            if (!string.IsNullOrWhiteSpace(ThemeButton.Image.Path))
                Button.Background = new ImageBrush(ConvertImage(ThemeButton.Image));
            else
                Button.Background = ConvertColor(ThemeButton.BackColor);

            Button.Foreground = ConvertColor(ThemeButton.TextColor);
            Button.FontFamily = new FontFamily(ThemeButton.Font.Name);
            Button.FontSize = ThemeButton.Font.Size;
            Button.Content = ThemeButton.Text;
        }

        public static void ApplyButton(Button Button, TYieldButton ThemeButton)
        {
            if (!string.IsNullOrWhiteSpace(ThemeButton.Image.Path))
                Button.Background = new ImageBrush(ConvertImage(ThemeButton.Image));
            else
                Button.Background = ConvertColor(ThemeButton.BackColor);

            Button.Foreground = ConvertColor(ThemeButton.TextColor);
            Button.FontFamily = new FontFamily(ThemeButton.Font.Name);
            Button.FontSize = ThemeButton.Font.Size;
            Button.Content = ThemeButton.TextNormal;
        }

        public static void ApplyButton(Button Button, TGlyphButton ThemeButton)
        {
            Button.Background = ConvertColor(ThemeButton.BackColor);
            Button.Foreground = ConvertColor(ThemeButton.GlyphColor);
        }

        public static void ApplySeperator(Grid Seperator, TSeperator ThemeSeperator)
        {
            Seperator.Visibility = ThemeSeperator.Enabled ? Visibility.Visible : Visibility.Hidden;
            Seperator.Background = ConvertColor(ThemeSeperator.BackColor);
        }

        public static void ApplyLabel(Label Label, TLabel ThemeLabel)
        {
            Label.Visibility = ThemeLabel.Enabled ? Visibility.Visible : Visibility.Hidden;
            Label.Background = ConvertColor(ThemeLabel.BackColor);
            Label.Foreground = ConvertColor(ThemeLabel.TextColor);
            Label.FontFamily = new FontFamily(ThemeLabel.Font.Name);
            Label.FontSize = ThemeLabel.Font.Size;
            Label.Content = ThemeLabel.Text;
        }

        public static void ApplyFormatLabel(Label Label, TFormatLabel ThemeLabel, string Version)
        {
            Label.Visibility = ThemeLabel.Enabled ? Visibility.Visible : Visibility.Hidden;
            Label.Background = ConvertColor(ThemeLabel.BackColor);
            Label.Foreground = ConvertColor(ThemeLabel.TextColor);
            Label.FontFamily = new FontFamily(ThemeLabel.Font.Name);
            Label.FontSize = ThemeLabel.Font.Size;
            Label.Content = ThemeLabel.FormatString.Replace("{version}", Version);
        }

        public static void ApplyWindow(Window Form, TForm ThemeForm)
        {
            if (!string.IsNullOrWhiteSpace(ThemeForm.Image.Path))
                Form.Background = new ImageBrush(ConvertImage(ThemeForm.Image));
            else
                Form.Background = ConvertColor(ThemeForm.BackColor);

            Form.Topmost = ThemeForm.TopMost;
            Form.Opacity = ThemeForm.Opacity;
        }

        public static void ApplyListBox(ListBox ListBox, TListBox ThemeListBox)
        {
            ListBox.Background = ConvertColor(ThemeListBox.BackColor);
            ListBox.Foreground = ConvertColor(ThemeListBox.TextColor);
            ListBox.FontFamily = new FontFamily(ThemeListBox.Font.Name);
            ListBox.FontSize = ThemeListBox.Font.Size;
        }

        public static void ApplyTextBox(TextBox ListBox, TListBox ThemeListBox)
        {
            ListBox.Background = ConvertColor(ThemeListBox.BackColor);
            ListBox.Foreground = ConvertColor(ThemeListBox.TextColor);
            ListBox.FontFamily = new FontFamily(ThemeListBox.Font.Name);
            ListBox.FontSize = ThemeListBox.Font.Size;
        }

        public static void ApplyLogo(System.Windows.Controls.Image LogoBox, TLogo ThemeLogoBox)
        {
            if (ThemeLogoBox.Image.Path != "") LogoBox.Source = ConvertImage(ThemeLogoBox.Image);
        }

        public static TBase Default()
        {
            var Base = new TBase
            {
                Version = 2
            };

            var Load = new TLoad
            {
                Base = new TForm {BackColor = new TColor(255, 51, 51, 51), Image = new TImage(), TopMost = true, Opacity = 1d},
                BaseStrings =
                    new TInitStrings
                    {
                        CheckingWhitelist = "Checking whitelist...",
                        ChangingWhitelist = "Changing whitelist...",
                        DownloadingData = "Downloading data...",
                        CheckingData = "Checking data...",
                        DownloadingDlls = "Downloading DLLs...",
                        DownloadingMonaco = "Downloading Monaco...",
                        DownloadingCefSharp = "Downloading CefSharp...",
                        DownloadingSQLite = "Downloading SQLite...",
                        JoiningDiscord = "Joining Discord...",
                        Ready = "Ready to launch!"
                    },
                Logo = new TLogo { Image = new TImage() },
                StatusBox = new TLabel
                {
                    Enabled = true, BackColor = new TColor(255, 51, 51, 51), Font = new TFont("Segoe UI", 14f), TextColor = new TColor(255, 255, 255, 255), Text = "Initializing..."
                },
                TitleBox = new TLabel
                {
                    Enabled = true, BackColor = new TColor(255, 60, 60, 60), Font = new TFont("Segoe UI", 12f), TextColor = new TColor(255, 255, 255, 255), Text = "Synapse X - Loader"
                },
                TopBox = new TSeperator {Enabled = true, BackColor = new TColor(255, 60, 60, 60) }
            };

            Base.Load = Load;

            var Main = new TMain
            {
                Base = new TForm {BackColor = new TColor(255, 51, 51, 51), Image = new TImage(), TopMost = true, Opacity = 1d },
                WebSocket = new TWebSocket { Enabled = false, DebugMode = false },
                Editor = new TEditor { Light = false, FixPixel = new TColor(255, 30, 30, 30) },
                BaseStrings =
                    new TAttachStrings
                    {
                        FailedToFindRoblox = " (failed to find roblox!)",
                        AlreadyInjected = " (already injected!)",
                        NotInjected = " (not injected! press attach)",
                        NotRunningLatestVersion = " (not running latest version! relaunch.)",
                        Checking = " (checking...)",
                        Injecting = " (injecting...)",
                        CheckingWhitelist = " (checking whitelist...)",
                        Scanning = " (scanning...)",
                        Ready = " (ready!)"
                    },
                RightClickStrings =
                    new TRightClickStrings
                    {
                        Execute = "Execute",
                        LoadToEditor = "Load to Editor",
                        Refresh = "Refresh"
                    },
                Logo = new TLogo { Image = new TImage() },
                TopBox = new TSeperator {Enabled = true, BackColor = new TColor(255, 60, 60, 60)},
                TitleBox =
                    new TFormatLabel
                    {
                        Enabled = true,
                        BackColor = new TColor(255, 60, 60, 60),
                        Font = new TFont("Segoe UI", 12f),
                        FormatString = "Synapse X - {version}",
                        TextColor = new TColor(255, 255, 255, 255)
                    },
                ScriptBox =
                    new TListBox
                    {
                        BackColor = new TColor(255, 60, 60, 60),
                        TextColor = new TColor(255, 255, 255, 255),
                        Font = new TFont("Segoe UI", 12f)
                    },
                MinimizeButton =
                    new TGlyphButton {BackColor = new TColor(255, 60, 60, 60), GlyphColor = new TColor(255, 255, 255, 255)},
                ExitButton =
                    new TGlyphButton {BackColor = new TColor(255, 60, 60, 60), GlyphColor = new TColor(255, 255, 255, 255)},
                ExecuteButton =
                    new TButton
                    {
                        BackColor = new TColor(255, 60, 60, 60),
                        TextColor = new TColor(255, 255, 255, 255),
                        Font = new TFont("Segoe UI", 14f),
                        Image = new TImage(),
                        Text = "Execute"
                    },
                ClearButton =
                    new TButton
                    {
                        BackColor = new TColor(255, 60, 60, 60),
                        TextColor = new TColor(255, 255, 255, 255),
                        Font = new TFont("Segoe UI", 14f),
                        Image = new TImage(),
                        Text = "Clear"
                    },
                OpenFileButton =
                    new TButton
                    {
                        BackColor = new TColor(255, 60, 60, 60),
                        TextColor = new TColor(255, 255, 255, 255),
                        Font = new TFont("Segoe UI", 14f),
                        Image = new TImage(),
                        Text = "Open File"
                    },
                ExecuteFileButton =
                    new TButton
                    {
                        BackColor = new TColor(255, 60, 60, 60),
                        TextColor = new TColor(255, 255, 255, 255),
                        Font = new TFont("Segoe UI", 14f),
                        Image = new TImage(),
                        Text = "Execute File"
                    },
                SaveFileButton =
                    new TButton
                    {
                        BackColor = new TColor(255, 60, 60, 60),
                        TextColor = new TColor(255, 255, 255, 255),
                        Font = new TFont("Segoe UI", 14f),
                        Image = new TImage(),
                        Text = "Save File"
                    },
                OptionsButton =
                    new TButton
                    {
                        BackColor = new TColor(255, 60, 60, 60),
                        TextColor = new TColor(255, 255, 255, 255),
                        Font = new TFont("Segoe UI", 14f),
                        Image = new TImage(),
                        Text = "Options"
                    },
                AttachButton =
                    new TButton
                    {
                        BackColor = new TColor(255, 60, 60, 60),
                        TextColor = new TColor(255, 255, 255, 255),
                        Font = new TFont("Segoe UI", 14f),
                        Image = new TImage(),
                        Text = "Attach"
                    },
                ScriptHubButton = new TYieldButton
                {
                    BackColor = new TColor(255, 60, 60, 60),
                    TextColor = new TColor(255, 255, 255, 255),
                    Font = new TFont("Segoe UI", 14f),
                    Image = new TImage(),
                    TextNormal = "Script Hub",
                    TextYield = "Starting..."
                }
            };

            Base.Main = Main;

            var ScriptHub = new TScriptHub
            {
                Base = new TForm { BackColor = new TColor(255, 51, 51, 51), Image = new TImage(), TopMost = true, Opacity = 1d },
                Logo = new TLogo { Image = new TImage() },
                TopBox = new TSeperator {Enabled = true, BackColor = new TColor(255, 60, 60, 60) },
                TitleBox =
                    new TLabel
                    {
                        Enabled = true, BackColor = new TColor(255, 60, 60, 60), Font = new TFont("Segoe UI", 12f), TextColor = new TColor(255, 255, 255, 255), Text = "Synapse X - Script Hub"
                    },
                ScriptBox =
                    new TListBox
                    {
                        BackColor = new TColor(255, 30, 30, 30),
                        TextColor = new TColor(255, 255, 255, 255),
                        Font = new TFont("Segoe UI", 12f)
                    },
                DescriptionBox =
                    new TListBox
                    {
                        BackColor = new TColor(255, 30, 30, 30),
                        TextColor = new TColor(255, 255, 255, 255),
                        Font = new TFont("Segoe UI", 12f)
                    },
                MinimizeButton =
                    new TGlyphButton {BackColor = new TColor(255, 60, 60, 60), GlyphColor = new TColor(255, 255, 255, 255)},
                ExecuteButton = new TYieldButton
                {
                    BackColor = new TColor(255, 60, 60, 60),
                    TextColor = new TColor(255, 255, 255, 255),
                    Font = new TFont("Segoe UI", 12f),
                    Image = new TImage(),
                    TextNormal = "Execute",
                    TextYield = "Executing..."
                },
                CloseButton = new TButton
                {
                    BackColor = new TColor(255, 60, 60, 60),
                    TextColor = new TColor(255, 255, 255, 255),
                    Font = new TFont("Segoe UI", 12f),
                    Image = new TImage(),
                    Text = "Close"
                }
            };

            Base.ScriptHub = ScriptHub;

            return Base;
        }
    }
}
