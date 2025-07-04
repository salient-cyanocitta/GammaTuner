using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace GammaTuner
{
    public class AppSettings
    {
        private static readonly string settingsFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public SettingsModel Settings
        {
            get
            {
                return settings;
            }
            set {
                settings = value;
            }
        }
        private SettingsModel settings = new SettingsModel();

        public class SettingsModel : ICloneable
        {
            public bool RunOnStartup { get; set; } = false;
            public bool MinimizeToTray { get; set; } = false;
            public bool StartMinimized { get; set; } = false;

            public bool LoadSettingsFileHDR { get; set; } = false;
            public bool LoadSettingsFileSDR { get; set; } = false;
            public string HDRSettingsFilePath { get; set; } = "";
            public string SDRSettingsFilePath { get; set; } = "";
            public bool ApplyUponSwitchingToSDRHDR { get; set; } = false;
            public bool ContinuouslyReapply { get; set; } = false;
            public int PollingInterval { get; set; } = 1000;

            public bool ReduceCPUUsage { get; set; } = true;

            public object Clone()
            {
                var clone = new SettingsModel();
                clone.RunOnStartup = this.RunOnStartup;
                clone.MinimizeToTray = this.MinimizeToTray;
                clone.StartMinimized = this.StartMinimized;
                clone.LoadSettingsFileHDR = this.LoadSettingsFileHDR;
                clone.LoadSettingsFileSDR = this.LoadSettingsFileSDR;
                clone.HDRSettingsFilePath = this.HDRSettingsFilePath;
                clone.SDRSettingsFilePath = this.SDRSettingsFilePath;
                clone.ApplyUponSwitchingToSDRHDR = this.ApplyUponSwitchingToSDRHDR;
                clone.ContinuouslyReapply = this.ContinuouslyReapply;
                clone.PollingInterval = this.PollingInterval;
                clone.ReduceCPUUsage = this.ReduceCPUUsage;
                return clone;
            }

            public override bool Equals(object? obj)
            {
                return obj is SettingsModel model &&
                       RunOnStartup == model.RunOnStartup &&
                       MinimizeToTray == model.MinimizeToTray &&
                       StartMinimized == model.StartMinimized &&
                       LoadSettingsFileHDR == model.LoadSettingsFileHDR &&
                       LoadSettingsFileSDR == model.LoadSettingsFileSDR &&
                       HDRSettingsFilePath == model.HDRSettingsFilePath &&
                       SDRSettingsFilePath == model.SDRSettingsFilePath &&
                       ApplyUponSwitchingToSDRHDR == model.ApplyUponSwitchingToSDRHDR &&
                       ContinuouslyReapply == model.ContinuouslyReapply &&
                       PollingInterval == model.PollingInterval &&
                       ReduceCPUUsage == model.ReduceCPUUsage;
            }

            public override int GetHashCode()
            {
                HashCode hash = new HashCode();
                hash.Add(RunOnStartup);
                hash.Add(MinimizeToTray);
                hash.Add(StartMinimized);
                hash.Add(LoadSettingsFileHDR);
                hash.Add(LoadSettingsFileSDR);
                hash.Add(HDRSettingsFilePath);
                hash.Add(SDRSettingsFilePath);
                hash.Add(ApplyUponSwitchingToSDRHDR);
                hash.Add(ContinuouslyReapply);
                hash.Add(PollingInterval);
                hash.Add(ReduceCPUUsage);
                return hash.ToHashCode();
            }
        }

        public void Load()
        {
            if (File.Exists(settingsFilePath))
            {
                string json = File.ReadAllText(settingsFilePath);
                var loaded = JsonSerializer.Deserialize<SettingsModel>(json);
                if (loaded != null)
                    settings = loaded;
            }
        }

        public void Save()
        {
            Debug.WriteLine("Saved settings");
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, json);
        }

        public new void GetHashCode()
        {
            throw new NotImplementedException("GetHashCode is not implemented for AppSettings. Use SettingsModel instead.");
        }
    }
}