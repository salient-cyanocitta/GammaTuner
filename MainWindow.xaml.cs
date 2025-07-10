using Microsoft.Win32;
using ScottPlot;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Colors = ScottPlot.Colors;
using Point = System.Windows.Point;

namespace GammaTuner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppSettings settings;
        public AppSettings AppSettings
        {
            get { return settings; }
            set { settings = value; }
        }


        private ScottPlot.Plottables.Scatter ScatterR;
        private ScottPlot.Plottables.Scatter ScatterG;
        private ScottPlot.Plottables.Scatter ScatterB;

        int? IndexBeingDragged = null;

        Gamma gamma;

        Backend backend;
        public Backend Backend { get { return backend; } }
        public Gamma Gamma
        {
            get
            {
                return gamma;
            }
        }

        int[] xAxis;

        void rebindPlot()
        {
            if(ScatterR != null)
                WpfPlot1.Plot.Remove(ScatterR);
            if (ScatterG != null)
                WpfPlot1.Plot.Remove(ScatterG);
            if (ScatterB != null)
                WpfPlot1.Plot.Remove(ScatterB);
            ScatterR = WpfPlot1.Plot.Add.Scatter(xAxis, _currentChartState.Gamma.R);
            ScatterR.LineColor = Colors.Red;
            ScatterG = WpfPlot1.Plot.Add.Scatter(xAxis, _currentChartState.Gamma.G);
            ScatterG.LineColor = Colors.Green;
            ScatterB = WpfPlot1.Plot.Add.Scatter(xAxis, _currentChartState.Gamma.B);
            ScatterR.LineColor = Colors.Blue;
        }

        

        /// <summary>
        /// Chart values are bound to the values in this
        /// </summary>
        ChartState CurrentChartState
        {
            get
            {
                if (_currentChartState == null) _currentChartState = GetDefaultChartStateDeepCopy();
                return _currentChartState;
            }
            set
            {
                _currentChartState = value;
                rebindPlot();
            }
        }
        /// <summary>
        /// Update the chart via <see cref="CurrentChartState"/> instead of this variable
        /// </summary>
        ChartState _currentChartState;

        ChartState GetDefaultChartStateDeepCopy()
        {
            return new ChartState(GetDefaultChartGamma())
            {
                GammaOffsetB = 1.00,
                GammaOffsetG = 1.00,
                GammaOffsetR = 1.00
            };
        }

        ChartGamma GetDefaultChartGamma()
        {
            var ramp = gamma.GetZeroedGammaRamp();
            int[] rampR = Array.ConvertAll(ramp.Red, val => checked((int)val));
            int[] rampG = Array.ConvertAll(ramp.Green, val => checked((int)val));
            int[] rampB = Array.ConvertAll(ramp.Blue, val => checked((int)val));

            return new ChartGamma(rampR, rampG, rampB);
        }
        public record ChartState
        {
            public readonly ChartGamma Gamma;
            public required double GammaOffsetR;
            public required double GammaOffsetG;
            public required double GammaOffsetB;

            /// <summary>
            /// Deep clones the passed <see cref="ChartGamma"/>
            /// </summary>
            /// <param name="chartGamma"></param>
            public ChartState(ChartGamma chartGamma)
            {
                Gamma = new ChartGamma(chartGamma);
            }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
            private ChartState() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        }

        ChartState DeepCloneCurrentChartState()
        {
            return new ChartState(CurrentChartState.Gamma) {
                GammaOffsetR = CurrentChartState.GammaOffsetR,
                GammaOffsetG = CurrentChartState.GammaOffsetG,
                GammaOffsetB = CurrentChartState.GammaOffsetB,
            };
        }

        class FalloffMap
        {
            public static double Linear(double value, double x)
            {
                if (x<0 || x>1)
                {
                    throw new ArgumentException("x is not in the range [0, 1]");
                }
                return (1-x) * value;
            }
            public static double Exponential(double value, double x)
            {
                if (x < 0 || x > 1)
                {
                    throw new ArgumentException("x is not in the range [0, 1]");
                }
                return Math.Pow(x-1, 2) * value;
            }

            public static double InverseExponential(double value, double x)
            {
                if (x < 0 || x > 1)
                {
                    throw new ArgumentException("x is not in the range [0, 1]");
                }
                return (-Math.Pow(x, 2) + 1) * value;
            }

            public static double Cosine(double value, double x)
            {
                if (x < 0 || x > 1)
                {
                    throw new ArgumentException("x is not in the range [0, 1]");
                }
                return (Math.Cos(x * Math.PI)/2 + 0.5) * value;
            }
        }

        private readonly Stopwatch appTimer = Stopwatch.StartNew();
        public double time => appTimer.Elapsed.TotalSeconds;

        public bool DataInitialized
        {
            get;
            private set;
        }
        public MainWindow()
        {
            //ORDER MATTERS HERE

            InitializeComponent();

            settings = new AppSettings();
            settings.Load();

            gamma = new Gamma();

            InitializeTrayIcon();

            // START MINIMIZED LOGIC
            if (settings.Settings.StartMinimized)
            {
                WindowState = WindowState.Minimized;
                // If MinimizeToTray is also enabled, hide the window
                if (settings.Settings.MinimizeToTray)
                {
                    Hide();
                }
            }

            var initialGamma = gamma.GetZeroedGammaRamp();
            int gammaLength = initialGamma.Red.Length;

            _currentChartState = GetDefaultChartStateDeepCopy();
            originalGamma = _currentChartState.Gamma;
            runningGammaTask = Task.CompletedTask;

            xAxis = new int[256];
            for (int i = 0; i < xAxis.Length; i++)
            {
                xAxis[i] = i;
            }

            prevChartStates.Push(CurrentChartState);

            backend = new Backend(this, settings, gamma);
            backend.ApplySettingsAndRun();

            ScatterR = WpfPlot1.Plot.Add.Scatter(xAxis, CurrentChartState.Gamma.R);
            ScatterG = WpfPlot1.Plot.Add.Scatter(xAxis, CurrentChartState.Gamma.G);
            ScatterB = WpfPlot1.Plot.Add.Scatter(xAxis, CurrentChartState.Gamma.B);

            DataInitialized = true;

            //WpfPlot1.Plot.Axes.SetLimits(0, 255, 0, 255);
            //WpfPlot1.Plot.Axes.SetLimitsY(0, ushort.MaxValue);

            double initialLimitFactor = 0.2; //show the bottom left 20% of the curve
            WpfPlot1.Plot.Axes.SetLimits(0, initialLimitFactor * 255, 0, initialLimitFactor * ushort.MaxValue);

            WpfPlot1.MouseDown += FormMouseDown;
            WpfPlot1.MouseUp += FormMouseUp;
            WpfPlot1.MouseMove += FormMouseMove;
            WpfPlot1.MouseWheel += FormMouseWheel;
            WpfPlot1.KeyDown += WpfPlot1_KeyDown;
            WpfPlot1.KeyUp += WpfPlot1_KeyUp;

            ApplyScaling();

            WpfPlot1.Refresh();
        }

        void ApplyScaling()
        {
            foreach (var axis in WpfPlot1.Plot.Axes.GetXAxes())
            {
                axis.TickLabelStyle.FontSize *= WpfPlot1.DisplayScale;
            }
            foreach (var axis in WpfPlot1.Plot.Axes.GetYAxes())
            {
                axis.TickLabelStyle.FontSize *= WpfPlot1.DisplayScale;
            }
            foreach (var plottable in WpfPlot1.Plot.GetPlottables())
            {
                if (plottable is ScottPlot.Plottables.Scatter scatter)
                {
                    scatter.MarkerSize *= WpfPlot1.DisplayScale;
                    scatter.LineWidth *= WpfPlot1.DisplayScale;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus(); // Ensure the window has keyboard focus on load
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl) ctrlDown = true;
            if (e.Key == Key.LeftShift) shiftDown = true;

            if (ctrlDown && e.Key == Key.Z)
            {
                if (shiftDown)
                {
                    if (Redo())
                        Debug.WriteLine("Redo");
                    else
                        Debug.WriteLine("Nothing to redo");
                }
                else
                {
                    Debug.WriteLine("Undo");
                    Undo();
                }

                e.Handled = true;
            }

            if (ctrlDown && e.Key == Key.Y)
            {
                if (Redo())
                    Debug.WriteLine("Redo");
                else
                    Debug.WriteLine("Nothing to redo");

                e.Handled = true;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl) ctrlDown = false;
            if (e.Key == Key.LeftShift) shiftDown = false;
        }

        bool ctrlDown = false;
        bool shiftDown = false;
        private void WpfPlot1_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void WpfPlot1_KeyDown(object sender, KeyEventArgs e)
        {
        }

        Stack<ChartState> redoAbleChartStates = new();
        Stack<ChartState> prevChartStates = new();
        private void Undo()
        {
            if (prevChartStates.Count > 0)
            {
                var prevState = prevChartStates.Pop();

                redoAbleChartStates.Push(DeepCloneCurrentChartState());
                CurrentChartState = new ChartState(prevState.Gamma){ 
                    GammaOffsetR = prevState.GammaOffsetR,
                    GammaOffsetG = prevState.GammaOffsetG,
                    GammaOffsetB = prevState.GammaOffsetB,
                };
            } else
            {
                Debug.WriteLine("Nothing to undo. Restoring original");
                CurrentChartState = GetDefaultChartStateDeepCopy();
            }

            UpdateUI();

            if (instantApply)
                SetGamma();
        }

        private bool Redo()
        {
            if (redoAbleChartStates.Count > 0)
            {
                var redoed = redoAbleChartStates.Pop();
                CurrentChartState = new ChartState(redoed.Gamma)
                {
                    GammaOffsetR = redoed.GammaOffsetR,
                    GammaOffsetG = redoed.GammaOffsetG,
                    GammaOffsetB = redoed.GammaOffsetB,
                };
                prevChartStates.Push(redoed);
            }
            else return false;

            UpdateUI();

            if (instantApply)
                SetGamma();

            return true;
        }

        void AddUndoRedoPoint()
        {
            prevChartStates.Push(DeepCloneCurrentChartState());
            Debug.WriteLine("Added Undo/Redo point");
        }

        private void UpdateUI()
        {
            WpfPlot1.Refresh();
            UpdateSliderValues();
            UpdateSliderLabels();
        }

        private void UpdateSliderValues()
        {
            RedSlider.Value = CurrentChartState.GammaOffsetR;
            GreenSlider.Value = CurrentChartState.GammaOffsetG;
            BlueSlider.Value = CurrentChartState.GammaOffsetB;
        }

        bool mouseDown = false;

        double dragRange = 20;
        private ScottPlot.Plottables.Ellipse? dragCircle;
        private void FormMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (mouseDown)
            {
                WpfPlot1.UserInputProcessor.Disable();

                WpfPlot1.Plot.Axes.SetLimits(axisLimitsOnMouseDown);
                WpfPlot1.Refresh();

                dragRange -= e.Delta / 40;
                if (dragRange < 1) dragRange = 1;

                var position = e.GetPosition(WpfPlot1);
                DrawSelectCircle(position);
                SetDraggedPoints(position);
            }

            WpfPlot1.Refresh();
        }

        void DrawSelectCircle(Point position)
        {
            if (dragCircle != null)
            {
                WpfPlot1.Plot.Remove(dragCircle);
                dragCircle = null;
            }

            Pixel mousePixel = new(position.X * WpfPlot1.DisplayScale, position.Y * WpfPlot1.DisplayScale);
            Coordinates mouseLocation = WpfPlot1.Plot.GetCoordinates(mousePixel);

            var xRange = Math.Abs(WpfPlot1.Plot.Axes.GetLimits().XRange.Span);
            var yRange = Math.Abs(WpfPlot1.Plot.Axes.GetLimits().YRange.Span);

            dragCircle = WpfPlot1.Plot.Add.Ellipse(
                center: mouseLocation,
                radiusX: dragRange,
                radiusY: yRange / xRange * dragRange
            );
        }

        bool isDraggingPoint = false;
        private void FormMouseUp(object? sender, MouseEventArgs e)
        {
            mouseDown = false;
            IndexBeingDragged = null;
            WpfPlot1.UserInputProcessor.Enable();
            SetLimits();       

            WpfPlot1.Refresh();
        }

        double lastTimeAppliedGamma = 0;
        double gammaDebounce = 0.01;
        private void FormMouseMove(object? sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(WpfPlot1);

            DrawSelectCircle(mousePosition);

            WpfPlot1.Refresh();

            Pixel mousePixel = new(mousePosition.X * WpfPlot1.DisplayScale, mousePosition.Y * WpfPlot1.DisplayScale);
            Coordinates mouseLocation = WpfPlot1.Plot.GetCoordinates(mousePixel);
            DataPoint nearestR = ScatterR.Data.GetNearest(mouseLocation, WpfPlot1.Plot.LastRender);
            DataPoint nearestG = ScatterR.Data.GetNearest(mouseLocation, WpfPlot1.Plot.LastRender);
            DataPoint nearestB = ScatterR.Data.GetNearest(mouseLocation, WpfPlot1.Plot.LastRender);

            WpfPlot1.Cursor = nearestR.IsReal ? Cursors.Hand : Cursors.Arrow;

            if (!mouseDown) return;

            SetDraggedPoints(mousePosition); //the juicy bit!!

            SetLimits();

            WpfPlot1.Refresh();
        }

        ChartGamma originalGamma; //set on mouse down, used to reset the chart gamma when dragging points
        AxisLimits axisLimitsOnMouseDown;
        private void FormMouseDown(object? sender, MouseEventArgs e)
        {
            if (originalGamma == null) originalGamma = GetDefaultChartGamma();

            axisLimitsOnMouseDown = WpfPlot1.Plot.Axes.GetLimits();

            mouseDown = true;

            originalGamma = new ChartGamma(CurrentChartState.Gamma);

            if (isDraggingPoint)
            {
                Debug.WriteLine("Added chart state");
                AddUndoRedoPoint();
                isDraggingPoint = false;
            }

            WpfPlot1.Refresh();

            SetLimits();
        }
        void SetDraggedPoints(Point mousePosition)
        {
            Pixel mousePixel = new(mousePosition.X * WpfPlot1.DisplayScale, mousePosition.Y * WpfPlot1.DisplayScale);
            Coordinates mouseLocation = WpfPlot1.Plot.GetCoordinates(mousePixel);
            DataPoint nearestR = ScatterR.Data.GetNearest(mouseLocation, WpfPlot1.Plot.LastRender);
            DataPoint nearestG = ScatterR.Data.GetNearest(mouseLocation, WpfPlot1.Plot.LastRender);
            DataPoint nearestB = ScatterR.Data.GetNearest(mouseLocation, WpfPlot1.Plot.LastRender);

            IndexBeingDragged = nearestR.IsReal ? nearestR.Index : null;

            if (IndexBeingDragged.HasValue && dragCircle != null)
            {
                dragCircle.Center = mouseLocation;
            }   

            if (IndexBeingDragged.HasValue)
            {
                WpfPlot1.UserInputProcessor.Disable();

                isDraggingPoint = true;

                var start = (int)Math.Max(0, IndexBeingDragged.Value - dragRange);
                var end = (int)Math.Min(255, IndexBeingDragged.Value + dragRange);

                var valueAtMouse = originalGamma.R[IndexBeingDragged.Value];

                //lerp the points based on distance to the mouse
                for (int i = start; i < end; i++)
                {
                    if (ignoreZero && i == 0) continue;

                    var initial = originalGamma.R[i];
                    var dist = Math.Abs(i - IndexBeingDragged.Value);

                    CurrentChartState.Gamma.R[i] = initial + (int)Math.Round((mouseLocation.Y - valueAtMouse) * FalloffMap.Cosine(1, dist / dragRange), MidpointRounding.AwayFromZero);
                    CurrentChartState.Gamma.G[i] = initial + (int)Math.Round((mouseLocation.Y - valueAtMouse) * FalloffMap.Cosine(1, dist / dragRange), MidpointRounding.AwayFromZero);
                    CurrentChartState.Gamma.B[i] = initial + (int)Math.Round((mouseLocation.Y - valueAtMouse) * FalloffMap.Cosine(1, dist / dragRange), MidpointRounding.AwayFromZero);
                    if (CurrentChartState.Gamma.R[i] < 0) CurrentChartState.Gamma.R[i] = 0;
                    if (CurrentChartState.Gamma.R[i] > ushort.MaxValue) CurrentChartState.Gamma.R[i] = ushort.MaxValue;
                    if (CurrentChartState.Gamma.G[i] < 0) CurrentChartState.Gamma.G[i] = 0;
                    if (CurrentChartState.Gamma.G[i] > ushort.MaxValue) CurrentChartState.Gamma.G[i] = ushort.MaxValue;
                    if (CurrentChartState.Gamma.B[i] < 0) CurrentChartState.Gamma.B[i] = 0;
                    if (CurrentChartState.Gamma.B[i] > ushort.MaxValue) CurrentChartState.Gamma.B[i] = ushort.MaxValue;
                }

                //reset the points outside the drag range
                for (int i = 0; i < start; i++)
                {
                    CurrentChartState.Gamma.R[i] = originalGamma.R[i];
                    CurrentChartState.Gamma.G[i] = originalGamma.G[i];
                    CurrentChartState.Gamma.B[i] = originalGamma.B[i];
                }
                for (int i = end; i < CurrentChartState.Gamma.Length; i++)
                {
                    CurrentChartState.Gamma.R[i] = originalGamma.R[i];
                    CurrentChartState.Gamma.G[i] = originalGamma.G[i];
                    CurrentChartState.Gamma.B[i] = originalGamma.B[i];
                }

                QueueSetGamma();
            }
        }

        Task runningGammaTask;
        void QueueSetGamma()
        {
            if (instantApply && time - lastTimeAppliedGamma > gammaDebounce)
            {
                lastTimeAppliedGamma = time;

                if (runningGammaTask == null || runningGammaTask.IsCompleted)
                    runningGammaTask = Task.Run(SetGamma);
                else if (runningGammaTask != null && runningGammaTask.Status == TaskStatus.Running)
                {
                    runningGammaTask = runningGammaTask.ContinueWith(_ => SetGamma(), TaskScheduler.Default);
                }
            }
        }

        private void SetLimits()
        {
            //just a placeholder until i figure out how to limit the chart to [0, 255] and [0, 65535]
        }

        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            CurrentChartState = GetDefaultChartStateDeepCopy();
            if (instantApply) SetGamma();
            UpdateUI();
            WpfPlot1.Refresh();
        }

        private void SetGamma_Click(object sender, RoutedEventArgs e)
        {
            SetGamma();
        }

        private void SetGamma()
        {
            gamma.TrySetGammaWithOffsets(
                CurrentChartState.Gamma,
                CurrentChartState.GammaOffsetR,
                CurrentChartState.GammaOffsetG,
                CurrentChartState.GammaOffsetB
            );
        }

        bool instantApply = true;
        bool ignoreZero = true;
        private void IgnoreZeroCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb)
                ignoreZero = cb.IsChecked == true;
        }

        private void InstantApplyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb)
                instantApply = cb.IsChecked == true;
        }

        private void ApplyDefaultGamma_Click(object sender, RoutedEventArgs e)
        {
            gamma.RevertGammaToUnity();
        }

        private void ImportCurveButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Import Gamma Curve"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ImportGamma(openFileDialog.FileName);
            }
            UpdateUI();
        }

        private void ExportCurveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Export Gamma Curve"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveGamma(saveFileDialog.FileName);
            }
        }

        private void SaveGamma(string fileName)
        {
            gamma.SaveGamma(
                fileName,
                CurrentChartState.Gamma,
                CurrentChartState.GammaOffsetR,
                CurrentChartState.GammaOffsetG,
                CurrentChartState.GammaOffsetB
            );
        }

        private void ImportGamma(string fileName)
        {
            try
            {
                var import = gamma.ImportGamma(fileName);
                if (import == null)
                {
                    MessageBox.Show("Invalid gamma file. Must contain R, G, and B arrays of length 256.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CurrentChartState = new ChartState(new ChartGamma(import.R, import.G, import.B))
                {
                    GammaOffsetR = import.GammaOffsetR,
                    GammaOffsetG = import.GammaOffsetG,
                    GammaOffsetB = import.GammaOffsetB
                };

                WpfPlot1.Refresh();

                if (instantApply)
                    SetGamma();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to import gamma curve:\n{ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void UpdateSliderLabels()
        {
            if(RedGammaLabel != null)
                RedGammaLabel.Content = $"Red Gamma: {(CurrentChartState.GammaOffsetR < 0 ? "" : "+")}{Math.Round(CurrentChartState.GammaOffsetR, 3)}";

            if (GreenGammaLabel != null)
                GreenGammaLabel.Content = $"Green Gamma: {(CurrentChartState.GammaOffsetG < 0 ? "" : "+")}{Math.Round(CurrentChartState.GammaOffsetG, 3)}";

            if (BlueGammaLabel != null)
                BlueGammaLabel.Content = $"Blue Gamma: {(CurrentChartState.GammaOffsetB < 0 ? "" : "+")}{Math.Round(CurrentChartState.GammaOffsetB, 3)}";
        }

        private void RedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(!DataInitialized) return;
            CurrentChartState.GammaOffsetR = (double)e.NewValue;
            UpdateSliderLabels();
            if (instantApply) QueueSetGamma();
        }

        private void GreenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!DataInitialized) return;
            CurrentChartState.GammaOffsetG = (double)e.NewValue;
            UpdateSliderLabels();
            if (instantApply) QueueSetGamma();
        }

        private void BlueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!DataInitialized) return;
            CurrentChartState.GammaOffsetB = (double)e.NewValue;
            UpdateSliderLabels();
            if (instantApply) QueueSetGamma();
        }

        SettingsWindow? settingsWindow;
        private void MoreSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (settingsWindow != null)
            {
                if (settingsWindow.IsVisible)
                {
                    settingsWindow.Activate();
                    return;
                }
                else
                {
                    settingsWindow = null;
                }
            }

            settingsWindow = new SettingsWindow(this);
            settingsWindow.Closed += (s, args) => settingsWindow = null;
            settingsWindow.Show();
        }

        private void UsefulInfoButton_Click(object sender, RoutedEventArgs e)
        {
            new ExternalResourcesWindow().ShowDialog();
        }

        private void SliderInteractionFinished(object sender, RoutedEventArgs e)
        {
            if (e is KeyEventArgs keyEvent)
            {
                if (keyEvent.Key == Key.Left || keyEvent.Key == Key.Right ||
                    keyEvent.Key == Key.Up || keyEvent.Key == Key.Down)
                {
                    AddUndoRedoPoint();
                }
            }
        }
    }
}