using System.Diagnostics;

namespace GammaTuner
{
    public  class Backend
    {
        private MainWindow mainWindow;
        private AppSettings appSettings;
        private Gamma gamma;
        public Backend(MainWindow mainWindow, AppSettings appSettings, Gamma gamma) 
        { 
            this.mainWindow = mainWindow;
            this.appSettings = appSettings;
            this.gamma = gamma;
        }

        Timer? refreshTimer;

        Gamma.GammaExportModel? SDRGammaModel;
        Gamma.GammaExportModel? HDRGammaModel;

        public void ApplySettingsAndRun()
        {
            if(refreshTimer != null)
            {
                refreshTimer.Dispose();
            }

            if (appSettings.Settings.ApplyUponSwitchingToSDRHDR) //we don't need the timer if this is off
                refreshTimer = new Timer(OnRefresh, null, 0, appSettings.Settings.PollingInterval);
            else 
                mainWindow.UseDefaultIcon(); 

            if (appSettings.Settings.LoadSettingsFileSDR)
            {
                try
                {
                    SDRGammaModel = gamma.ImportGamma(appSettings.Settings.SDRSettingsFilePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading SDR gamma settings file: {ex.Message}");
                    Utils.ErrorWindow($"Error loading SDR gamma settings file. Correct this through the settings window or JSON file where settings are stored: {ex.Message}");
                    SDRGammaModel = null;
                }
            }

            if (appSettings.Settings.LoadSettingsFileHDR)
            {
                try
                {
                    HDRGammaModel = gamma.ImportGamma(appSettings.Settings.HDRSettingsFilePath);
                } catch(Exception ex)
                {
                    Debug.WriteLine($"Error loading HDR gamma settings file: {ex.Message}");
                    Utils.ErrorWindow($"Error loading HDR gamma settings file. Correct this through the settings window or JSON file where settings are stored: {ex.Message}");
                    HDRGammaModel = null;
                }
            }

            if(appSettings.Settings.ApplyUponSwitchingToSDRHDR)
            {
                ApplyRespectiveGammaAccordingToHDROrSDR();
            }

            if(appSettings.Settings.RunOnStartup == true)
            {
                if (StartupShortcutManager.IsShortcutInStartup())
                {
                    Debug.WriteLine("App is already set to run on startup.");
                } else
                {
                    StartupShortcutManager.AddShortcutToStartup();
                    Debug.WriteLine("App added to startup.");
                }
            }
            else
            {
                StartupShortcutManager.RemoveShortcutFromStartup();
                Debug.WriteLine("Attempted to remove app from startup.");
            }

        }

        void ApplyRespectiveGammaAccordingToHDROrSDR()
        {
            bool isHDR = MonitorStatus.IsHDREnabled();
            mainWindow.UpdateIconBasedOnHDR();
            if (isHDR && HDRGammaModel != null)
            {
                gamma.TrySetGammaWithOffsets(new ChartGamma(HDRGammaModel.R, HDRGammaModel.G, HDRGammaModel.B),
                    HDRGammaModel.GammaOffsetR, HDRGammaModel.GammaOffsetG, HDRGammaModel.GammaOffsetB);
            }
            else if (!isHDR && SDRGammaModel != null)
            {
                gamma.TrySetGammaWithOffsets(new ChartGamma(SDRGammaModel.R, SDRGammaModel.G, SDRGammaModel.B),
                    SDRGammaModel.GammaOffsetR, SDRGammaModel.GammaOffsetG, SDRGammaModel.GammaOffsetB);
            }
        }

        bool prevIsHDR = false;
        private void OnRefresh(object? state)
        {
            bool isHDR = MonitorStatus.IsHDREnabled();
            if (isHDR != prevIsHDR)
            {
                Debug.WriteLine($"HDR status changed: {prevIsHDR} -> {isHDR}");
                prevIsHDR = isHDR;
                if (appSettings.Settings.ApplyUponSwitchingToSDRHDR)
                {
                    ApplyRespectiveGammaAccordingToHDROrSDR();
                }
            } else if (appSettings.Settings.ContinuouslyReapply)
            {
                ApplyRespectiveGammaAccordingToHDROrSDR();
            }
        }
        
    }
}
