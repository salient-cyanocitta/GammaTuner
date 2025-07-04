using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace GammaTuner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "YourUniqueAppName_Mutex";

            bool createdNew;
            _mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                // Instance already running
                MessageBox.Show("Application is already running.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }
            AddExceptionEventHandlers();

            base.OnStartup(e);
        }

        void AddExceptionEventHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Debug.WriteLine($"Unhandled exception: {e}");
                MessageBox.Show(
                $"An unexpected error occurred: {Utils.RedactFilePathsFromString(e.ToString())}",
                "NVIDIAGammaCurve Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //System.Windows.Forms.MessageBox.Show(
                //    $"An unexpected error occurred: {Utils.RedactFilePathsFromString(e.ExceptionObject)}", 
                //    "NVIDIAGammaCurve Error",
                //    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error, System.Windows.Forms.MessageBoxDefaultButton.Button1);
            };

            Application.Current.DispatcherUnhandledException += (sender, e) =>
            {
                Debug.WriteLine($"Dispatcher Unhandled Exception: {e}");
                MessageBox.Show(
                $"An unexpected error occurred: {Utils.RedactFilePathsFromString(e.Exception.ToString())}",
                "NVIDIAGammaCurve Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Debug.WriteLine($"Current Domain Unhandle Exception: {e}");
                MessageBox.Show(
                $"An unexpected error occurred: {Utils.RedactFilePathsFromString(e.ExceptionObject.ToString())}",
                "NVIDIAGammaCurve Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Debug.WriteLine($"Unobserved Task Exception: {e}");
                MessageBox.Show(
                $"An unexpected error occurred: {Utils.RedactFilePathsFromString(e.Exception.ToString())}",
                "NVIDIAGammaCurve Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
        }
    }

}
