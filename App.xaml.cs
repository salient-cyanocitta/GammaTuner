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
            const string mutexName = "GammaTuner_Mutex";

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
                try
                {
                    Debug.WriteLine($"Unhandled exception: {e}");
                    MessageBox.Show(
                    $"An unexpected error occurred: {Utils.RedactFilePathsFromString(e.ToString())}",
                    "GammaTuner Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Log.Error($"Unhandled exception: {Utils.RedactFilePathsFromString(e.ToString())}");
                } catch (Exception )
                {
                    //prevent infinite loop if the error occurs in the exception handler itself
                }
                //System.Windows.Forms.MessageBox.Show(
                //    $"An unexpected error occurred: {Utils.RedactFilePathsFromString(e.ExceptionObject)}", 
                //    "GammaTuner Error",
                //    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error, System.Windows.Forms.MessageBoxDefaultButton.Button1);
            };

            Application.Current.DispatcherUnhandledException += (sender, e) =>
            {
                try
                {
                    Debug.WriteLine($"Dispatcher Unhandled exception: {e}");
                    MessageBox.Show(
                    $"An unexpected error occurred: {Utils.RedactFilePathsFromString(e.ToString())}",
                    "GammaTuner Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Log.Error($"Unhandled exception: {Utils.RedactFilePathsFromString(e.ToString())}");
                }
                catch (Exception)
                {
                    //prevent infinite loop if the error occurs in the exception handler itself
                }
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                try
                {
                    Debug.WriteLine($"CurrentDomain Unhandled exception: {e}");
                    MessageBox.Show(
                    $"An unexpected error occurred: {Utils.RedactFilePathsFromString(e.ToString())}",
                    "GammaTuner Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Log.Error($"Unhandled exception: {Utils.RedactFilePathsFromString(e.ToString())}");
                }
                catch (Exception)
                {
                    //prevent infinite loop if the error occurs in the exception handler itself
                }
            };
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                try
                {
                    Debug.WriteLine($"Unobserved Task exception: {e}");
                    MessageBox.Show(
                    $"An unexpected error occurred: {Utils.RedactFilePathsFromString(e.ToString())}",
                    "GammaTuner Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Log.Error($"Unhandled exception: {Utils.RedactFilePathsFromString(e.ToString())}");
                }
                catch (Exception)
                {
                    //prevent infinite loop if the error occurs in the exception handler itself
                }
            };
        }
    }

}
