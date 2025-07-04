using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace GammaTuner
{
    public class Utils
    {

        public static string RedactFilePathsFromString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }

            // Pattern captures the directory and file name separately
            string pattern = @"(?<!\w)([a-zA-Z]:\\(?:[^\\\r\n]+\\)+)([^\\\r\n]+)|(/\b(?:[^/\r\n]+/)+)([^/\r\n]+)";

            return Regex.Replace(input, pattern, "<FILEPATH REDACTED>/$2$4");
        }

        public static void ErrorWindow(string message)
        {
            MessageBox.Show(RedactFilePathsFromString(message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
