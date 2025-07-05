using OpenTK.Windowing.GraphicsLibraryFramework;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WindowsDisplayAPI;
using System.Windows;

namespace GammaTuner
{
    public class Gamma
    {

        public Gamma(bool useCachedMonitor) {
            this.useCachedMonitor = useCachedMonitor;
        }
        public bool useCachedMonitor;

        //apparently windows treats 0-255 >> 8 as the unity (default) gamma ramp
        public DisplayGammaRamp GetWindowsUnityGammaRamp() {
            var gamma = new ushort[256];
            for (int i = 0; i < gamma.Length; i++)
            {
                gamma[i] = (ushort)(i << 8);
            }
            return new DisplayGammaRamp(gamma, gamma, gamma);
        }

        public DisplayGammaRamp GetZeroedGammaRamp()
        {
            var gamma = new ushort[256];

            for (int i = 0; i < gamma.Length; i++)
            {
                double normalized = i / 255.0;
                gamma[i] = (ushort)Math.Round(normalized * ushort.MaxValue, MidpointRounding.AwayFromZero);
            }

            return new DisplayGammaRamp(gamma, gamma, gamma);
        }

        Display? cachedWindowsDisplay;
        public void RevertGammaToUnity()
        {
            if (useCachedMonitor && cachedWindowsDisplay != null)
            {
                cachedWindowsDisplay.GammaRamp = GetWindowsUnityGammaRamp();
            }

            Display? windowsDisplay = GetWindowsDisplay();
            cachedWindowsDisplay = windowsDisplay;
            if (windowsDisplay == null)
            {
                throw new Exception("No display found. Cannot revert gamma.");
            }

            windowsDisplay.GammaRamp = GetWindowsUnityGammaRamp();
        }

        private bool SetSystemGamma(ushort[] gammaR, ushort[] gammaG, ushort[] gammaB)
        {
            if (gammaR.Length != 256)
            {
                throw new ArgumentException("gamma array R is not 256 long!");
            }

            if (gammaG.Length != 256)
            {
                throw new ArgumentException("gamma array G is not 256 long!");
            }

            if (gammaB.Length != 256)
            {
                throw new ArgumentException("gamma array B is not 256 long!");
            }

            foreach (var x in gammaR)
            {
                if (x < 0 || x > ushort.MaxValue)
                {
                    throw new ArgumentException("an element in the gamma array R is not in the range [0, 65535]!");
                }
            }
            foreach (var x in gammaG)
            {
                if (x < 0 || x > ushort.MaxValue)
                {
                    throw new ArgumentException("an element in the gamma array G is not in the range [0, 65535]!");
                }
            }
            foreach (var x in gammaB)
            {
                if (x < 0 || x > ushort.MaxValue)
                {
                    throw new ArgumentException("an element in the gamma array B is not in the range [0, 65535]!");
                }
            }

            if(useCachedMonitor && cachedWindowsDisplay != null)
            {
                cachedWindowsDisplay.GammaRamp = new DisplayGammaRamp(gammaR, gammaG, gammaB);
                return true;
            }

            try
            {
                Display? windowsDisplay = GetWindowsDisplay();
                if(windowsDisplay == null)
                {
                    throw new Exception("No display found. Cannot set gamma.");
                }
                cachedWindowsDisplay = windowsDisplay;
                windowsDisplay.GammaRamp = new DisplayGammaRamp(gammaR, gammaG, gammaB);
            } catch (Exception e)
            {
                throw new Exception("Error occured while attempting to set gamma: " + e);
            }


            return true;
        }
        private static Display? GetWindowsDisplay()
        {
            Display[] displays = Display.GetDisplays().ToArray();
            foreach (Display display in displays)
            {
                if (display.DisplayScreen.IsPrimary)
                {
                    return display;
                }
            }

            return null;
        }

        // Export/import model for gamma curves
        public class GammaExportModel
        {
            public int[] R { get; set; }
            public int[] G { get; set; }
            public int[] B { get; set; }
            public double GammaOffsetR { get; set; }
            public double GammaOffsetG { get; set; }
            public double GammaOffsetB { get; set; }
        }

        public void SaveGamma(string fileName, ChartGamma gamma, double gammaOffsetR, double gammaOffsetG, double gammaOffsetB)
        {
            var exportModel = new GammaExportModel
            {
                R = gamma.R,
                G = gamma.G,
                B = gamma.B,
                GammaOffsetR = gammaOffsetR,
                GammaOffsetG = gammaOffsetG,
                GammaOffsetB = gammaOffsetB
            };

            var json = JsonSerializer.Serialize(exportModel, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(fileName, json);
        }

        public GammaExportModel? ImportGamma(string fileName)
        {
            string json = File.ReadAllText(fileName);
            var model = JsonSerializer.Deserialize<GammaExportModel>(json);
            if (model == null)
            {
                throw new JsonException("Unable to read JSOn file");
            }
            if (model.R == null || model.G == null || model.B == null)
            {
                throw new JsonException("Invalid gamma file. Must contain R, G, and B arrays.");
            }
            if (model.GammaOffsetR < 0.4 || model.GammaOffsetR > 2.8)
            {
                throw new JsonException("Invalid gamma offset (red). Must be between 0.4 and 2.8.");
            }
            if (model.GammaOffsetG < 0.4 || model.GammaOffsetG > 2.8)
            {
                throw new JsonException("Invalid gamma offset (green). Must be between 0.4 and 2.8.");
            }
            if (model.GammaOffsetB < 0.4 || model.GammaOffsetB > 2.8)
            {
                throw new JsonException("Invalid gamma offset (green). Must be between 0.4 and 2.8.");
            }
            return model;
        }

        public void SetGammaWithOffsets(ChartGamma chartGamma, double gammaOffsetR, double gammaOffsetG, double gammaOffsetB)
        {
            int[] rampR = (int[])chartGamma.R.Clone();
            int[] rampG = (int[])chartGamma.G.Clone();
            int[] rampB = (int[])chartGamma.B.Clone();

            var defaultGamma = GetZeroedGammaRamp();

            for (int i = 0; i <= 255; i++)
            {
                double defaultR = defaultGamma.Red[i];
                double factor = Math.Pow(i / 255d, 1 / gammaOffsetR);
                double modifiedR = factor * ushort.MaxValue;
                double diff = Math.Round(modifiedR - defaultR, MidpointRounding.AwayFromZero);
                rampR[i] += (int)diff;
                rampR[i] = (int)Math.Clamp((double)rampR[i], 0, ushort.MaxValue);
            }
            for (int i = 0; i <= 255; i++)
            {
                double defaultG = defaultGamma.Green[i];
                double factor = Math.Pow(i / 255d, 1 / gammaOffsetG);
                double modifiedG = factor * ushort.MaxValue;
                double diff = Math.Round(modifiedG - defaultG, MidpointRounding.AwayFromZero);
                rampG[i] += (int)diff;
                rampG[i] = (int)Math.Clamp((double)rampG[i], 0, ushort.MaxValue);
            }
            for (int i = 0; i <= 255; i++)
            {
                double defaultB = defaultGamma.Blue[i];
                double factor = Math.Pow(i / 255d, 1 / gammaOffsetB);
                double modifiedB = factor * ushort.MaxValue;
                double diff = Math.Round(modifiedB - defaultB, MidpointRounding.AwayFromZero);
                rampB[i] += (int)diff;
                rampB[i] = (int)Math.Clamp((double)rampB[i], 0, ushort.MaxValue);
            }

            ushort[] _rampR = Array.ConvertAll(rampR, val => checked((ushort)val));
            ushort[] _rampG = Array.ConvertAll(rampG, val => checked((ushort)val));
            ushort[] _rampB = Array.ConvertAll(rampB, val => checked((ushort)val));

            SetSystemGamma(_rampR, _rampG, _rampB);
        }
    }
}
