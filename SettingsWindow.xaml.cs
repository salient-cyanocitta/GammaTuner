using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace GammaTuner
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        MainWindow mainWindow;

        private AppSettings prevSettings;
        private AppSettings settings;// this is used locally, not shared with the main window

        public SettingsWindow(MainWindow mainWindow)
        {
            mainWindow.Closed += (s, e) => Close();

            this.mainWindow = mainWindow;

            prevSettings = new AppSettings();
            settings = new AppSettings();

            prevSettings.Settings = (AppSettings.SettingsModel)mainWindow.AppSettings.Settings.Clone();
            settings.Settings = (AppSettings.SettingsModel)mainWindow.AppSettings.Settings.Clone();

            InitializeComponent();
            ApplySettingsToUI();
        }

        void UpdateApplyButton()
        {
            if(ApplySettingsButton == null) return; // If the button is not initialized, do nothing
            if (settings.Settings.GetHashCode() != prevSettings.Settings.GetHashCode())
            {
                ApplySettingsButton.IsEnabled = true;
            }
            else
            {
                ApplySettingsButton.IsEnabled = false;
            }
        }

        private void RunOnStartupCheckbox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            settings.Settings.RunOnStartup = RunOnStartupCheckbox.IsChecked == true;
            settings.Save();
            UpdateApplyButton();
        }

        private void MinimizeToTrayCheckbox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            settings.Settings.MinimizeToTray = MinimizeToTrayCheckbox.IsChecked == true;
            settings.Save();
            UpdateApplyButton();
        }

        private void LoadSettingsFileHDRCheckbox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            settings.Settings.LoadSettingsFileHDR = LoadSettingsFileHDRCheckbox.IsChecked == true;
            settings.Save();
            UpdateApplyButton();
        }

        private void LoadSettingsFileSDRCheckbox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            settings.Settings.LoadSettingsFileSDR = LoadSettingsFileSDRCheckbox.IsChecked == true;
            settings.Save();
            UpdateApplyButton();
        }

        private void HDRSettingsFileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (settings == null) return;
            if (sender is TextBox tb)
            {
                string path = tb.Text;
                bool exists = string.IsNullOrWhiteSpace(path) || File.Exists(path);
                if (exists)
                {
                    tb.ClearValue(TextBox.BackgroundProperty);
                    settings.Settings.HDRSettingsFilePath = path;
                    settings.Save();
                }
                else
                {
                    tb.Background = new SolidColorBrush(Color.FromRgb(255, 200, 200)); // light red
                }
                UpdateApplyButton();
                ApplySettingsButton.IsEnabled = exists && (settings.Settings.GetHashCode() != prevSettings.Settings.GetHashCode());
            }
        }

        private void SDRSettingsFileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (settings == null) return;
            if (sender is TextBox tb)
            {
                string path = tb.Text;
                bool exists = string.IsNullOrWhiteSpace(path) || File.Exists(path);
                if (exists)
                {
                    tb.ClearValue(TextBox.BackgroundProperty);
                    settings.Settings.SDRSettingsFilePath = path;
                    settings.Save();
                }
                else
                {
                    tb.Background = new SolidColorBrush(Color.FromRgb(255, 200, 200)); // light red
                }
                UpdateApplyButton();
                ApplySettingsButton.IsEnabled = exists && (settings.Settings.GetHashCode() != prevSettings.Settings.GetHashCode());
            }
            
        }

        private void SDRSettingsFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select SDR Settings File"
            };
            if (dlg.ShowDialog() == true)
            {
                settings.Settings.SDRSettingsFilePath = dlg.FileName;
                settings.Save();
                if (SDRSettingsFileTextBox != null)
                    SDRSettingsFileTextBox.Text = dlg.FileName;
            }
            UpdateApplyButton();
        }

        private void HDRSettingsFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select HDR Settings File"
            };
            if (dlg.ShowDialog() == true)
            {
                settings.Settings.HDRSettingsFilePath = dlg.FileName;
                settings.Save();
                if (HDRSettingsFileTextBox != null)
                    HDRSettingsFileTextBox.Text = dlg.FileName;
            }
            UpdateApplyButton();
        }

        private void ApplySettingsToUI()
        {
            RunOnStartupCheckbox.IsChecked = settings.Settings.RunOnStartup;
            MinimizeToTrayCheckbox.IsChecked = settings.Settings.MinimizeToTray;
            if (LoadSettingsFileHDRCheckbox != null)
                LoadSettingsFileHDRCheckbox.IsChecked = settings.Settings.LoadSettingsFileHDR;
            if (LoadSettingsFileSDRCheckbox != null)
                LoadSettingsFileSDRCheckbox.IsChecked = settings.Settings.LoadSettingsFileSDR;
            if (HDRSettingsFileTextBox != null)
                HDRSettingsFileTextBox.Text = settings.Settings.HDRSettingsFilePath ?? "";
            if (SDRSettingsFileTextBox != null)
                SDRSettingsFileTextBox.Text = settings.Settings.SDRSettingsFilePath ?? "";
            // New fields
            if (ApplyUponSwitchingToSDRHDRCheckbox != null)
                ApplyUponSwitchingToSDRHDRCheckbox.IsChecked = settings.Settings.ApplyUponSwitchingToSDRHDR;
            if (ContinuouslyReapplyCheckBox != null)
                ContinuouslyReapplyCheckBox.IsChecked = settings.Settings.ContinuouslyReapply;
            if (PollingIntervalTextBox != null)
                PollingIntervalTextBox.Text = settings.Settings.PollingInterval.ToString();
            if (StartMinimizedCheckBox != null)
                StartMinimizedCheckBox.IsChecked = settings.Settings.StartMinimized;
        }

        private void ApplyUponSwitchingToSDRHDRCheckbox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            settings.Settings.ApplyUponSwitchingToSDRHDR = ApplyUponSwitchingToSDRHDRCheckbox.IsChecked == true;
            settings.Save();
            UpdateApplyButton();
        }

        private void ContinuouslyReapplyCheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            settings.Settings.ContinuouslyReapply = ContinuouslyReapplyCheckBox.IsChecked == true;
            settings.Save();
            UpdateApplyButton();
        }

        private void PollingIntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (settings == null) return;
            if (sender is TextBox tb)
            {
                if (int.TryParse(tb.Text, out int val) && val > 0)
                {
                    settings.Settings.PollingInterval = val;
                    settings.Save();
                }
            }
            UpdateApplyButton();
        }

        private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.AppSettings.Settings = (AppSettings.SettingsModel)settings.Settings.Clone(); // Update the main window's settings reference
            mainWindow.Backend.ApplySettingsAndRun();
            prevSettings.Settings = (AppSettings.SettingsModel)settings.Settings.Clone(); // Update previous settings to current
            UpdateApplyButton();
        }

        private void StartMinimizedCheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (settings == null) return;
            settings.Settings.StartMinimized = StartMinimizedCheckBox.IsChecked == true;
            settings.Save();
            UpdateApplyButton();
        }
    }
}
