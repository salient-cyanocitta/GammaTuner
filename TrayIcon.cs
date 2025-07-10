using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;

namespace GammaTuner
{
    public partial class MainWindow
    {
        [NotNull] //InitializeTrayIcon should be called from the MainWindow entry point
        private NotifyIcon _notifyIcon;

        public readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

        EventHandler DoubleClickEvent;

        readonly string defaultIconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "defaultIcon.ico");
        readonly string hdrIconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hdrIcon.ico");
        readonly string sdrIconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sdrIcon.ico");
        private void InitializeTrayIcon()
        {
            DoubleClickEvent = (s, e) => ShowWindow();

            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("defaultIcon.ico"),
                Visible = true,
                Text = "GammaTuner",
                ContextMenuStrip = new ContextMenuStrip()
            };

            _notifyIcon.ContextMenuStrip.Items.Add("Open", null, (s, e) => ShowWindow());
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => ExitApplication());

            _notifyIcon.DoubleClick += DoubleClickEvent;
        }

        public void UpdateIconBasedOnHDR()
        {
            bool isHDR = MonitorStatus.IsHDREnabled();
            if (isHDR)
            {
                _notifyIcon.Icon = new Icon("hdrIcon.ico");
                _notifyIcon.Text = "GammaTuner - HDR Detected (or Advanced Color is enabled)";

            }
            else
            {
                _notifyIcon.Icon = new Icon("sdrIcon.ico");
                _notifyIcon.Text = "GammaTuner - SDR Detected";

            }
        }

        public void UseDefaultIcon()
        {
            _notifyIcon.Icon = new Icon("defaultIcon.ico");
            _notifyIcon.Text = "GammaTuner";

        }

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ExitApplication()
        {
            _notifyIcon.Dispose();
            Application.Current.Shutdown();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized && AppSettings.Settings.MinimizeToTray)
            {
                Hide(); // Hide the window when minimized
            }

            base.OnStateChanged(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //just a placeholder
            base.OnClosing(e);
        }
    }
}
