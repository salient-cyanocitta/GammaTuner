using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace GammaTuner
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ExternalResourcesWindow : Window
    {
        public ExternalResourcesWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.ThemeMode = mainWindow.ThemeMode;

            var rtb = this.MainTextBox;
            TextRange range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            range.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily("Calibri"));
            range.ApplyPropertyValue(TextElement.FontSizeProperty, 14 * (96.0 / 72.0));

        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true,
            });
            e.Handled = true;
        }

    }
}
