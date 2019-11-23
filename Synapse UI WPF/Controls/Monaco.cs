using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;
using CefSharp.Wpf.Internals;

namespace Synapse_UI_WPF.Controls
{
    public enum MonacoTheme
    {
        Light = 0,
        Dark = 1
    }

    public class MonacoSettings
    {
        public bool ReadOnly; // The ability to edit text.
        public bool AutoIndent; // Enables auto indentation & adjustment
        public bool Folding; // Enables code folding.
        public bool FontLigatures; // Enables font ligatures.
        public bool Links;  // Enables whether links are clickable & detectible.
        public bool MinimapEnabled; // Enables whether code minimap is enabled.
        public int LineHeight; // Set's the line height.
        public double FontSize; // Determine's the font size of the text.
        public string FontFamily; // Set's the font family for the editor.
        public string RenderWhitespace; // "none" | "boundary" | "all"
    }

    [Obfuscation(Feature = "renaming", Exclude = true, ApplyToMembers = false)]
    public class Monaco : ChromiumWebBrowser
    {
        public bool MonacoLoaded;
        public delegate void MonacoReadyDelegate();

        [Obfuscation(Feature = "renaming", Exclude = true)]
        public event MonacoReadyDelegate MonacoReady;

        public Monaco()
        {
            Address = $"file:///{Environment.CurrentDirectory.Replace("\\", "/")}/bin/Monaco.html";

            LoadingStateChanged += (sender, args) =>
            {
                if (args.IsLoading) return;

                MonacoLoaded = true;
                MonacoReady?.Invoke();
            };
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            e.Handled = true;

            var browser = GetBrowser();
            var modifiers = e.GetModifiers();
            var point = e.GetPosition(this);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                browser.GetHost().SendMouseClickEvent((int) point.X, (int)point.Y, MouseButtonType.Left, mouseUp: true, clickCount: 1, modifiers: modifiers);
            }

            browser.GetHost().SendMouseMoveEvent((int) point.X, (int)point.Y, true, modifiers);

            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Set's Monaco editor's theme to the selected Choice.
        /// </summary>
        /// <param name="theme"></param>
        public void SetTheme(MonacoTheme theme)
        {
            if (!MonacoLoaded) return;

            switch (theme)
            {
                case MonacoTheme.Dark:
                    this.ExecuteScriptAsync("SetTheme", "Dark");
                    break;
                case MonacoTheme.Light:
                    this.ExecuteScriptAsync("SetTheme", "Light");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }

        /// <summary>
        /// Set's the text of Monaco to the parameter text.
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            if (MonacoLoaded)
                this.ExecuteScriptAsync("SetText", text);
        }

        private object EvaluateScript(string script)
        {
            var Task = this.EvaluateScriptAsync(script);
            Task.Wait();
            var Resp = Task.Result;
            return Resp.Success ? Resp.Result ?? "" : Resp.Message;
        }

        /// <summary>
        /// Get's the text of Monaco and returns it.
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            if (!MonacoLoaded) return "";

            return (string) EvaluateScript("GetText();");
        }

        /// <summary>
        /// Appends the text of Monaco with the parameter text.
        /// </summary>
        /// <param name="text"></param>
        public void AppendText(string text)
        {
            if (MonacoLoaded)
                SetText(GetText() + text);
        }

        public void GoToLine(int lineNumber)
        {
            if (MonacoLoaded)
                this.ExecuteScriptAsync("SetScroll", lineNumber);
        }

        /// <summary>
        /// Refreshes the Monaco editor.
        /// </summary>
        public void EditorRefresh()
        {
            if (MonacoLoaded)
                this.ExecuteScriptAsync("Refresh");
        }

        /// <summary>
        /// Updates Monaco editor's settings with it's parameter structure.
        /// </summary>
        /// <param name="settings"></param>
        public void UpdateSettings(MonacoSettings settings)
        {
            if (!MonacoLoaded) return;

            this.ExecuteScriptAsync("SwitchMinimap", settings.MinimapEnabled);
            this.ExecuteScriptAsync("SwitchReadonly", settings.ReadOnly);
            this.ExecuteScriptAsync("SwitchRenderWhitespace", settings.RenderWhitespace);
            this.ExecuteScriptAsync("SwitchLinks", settings.Links);
            this.ExecuteScriptAsync("SwitchLineHeight", settings.LineHeight);
            this.ExecuteScriptAsync("SwitchFontSize", settings.FontSize);
            this.ExecuteScriptAsync("SwitchFolding", settings.Folding);
            this.ExecuteScriptAsync("SwitchAutoIndent", settings.AutoIndent);
            this.ExecuteScriptAsync("SwitchFontFamily", settings.FontFamily);
            this.ExecuteScriptAsync("SwitchFontLigatures", settings.FontLigatures);
        }

        /// <summary>
        /// Adds intellisense for the specified type.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="type"></param>
        /// <param name="description"></param>
        /// <param name="insert"></param>
        public void AddIntellisense(string label, string type, string description, string insert)
        {
            if (MonacoLoaded)
                this.ExecuteScriptAsync("AddIntellisense", label, type, description, insert);
        }

        /// <summary>
        /// Creates a syntax error symbol (squiggly red line) on the specific parameters in the editor.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <param name="endLine"></param>
        /// <param name="endColumn"></param>
        /// <param name="message"></param>
        public void ShowSyntaxError(int line, int column, int endLine, int endColumn, string message)
        {
            if (MonacoLoaded)
                this.ExecuteScriptAsync("ShowErr", line, column, endLine, endColumn, message);
        }
    }
}
