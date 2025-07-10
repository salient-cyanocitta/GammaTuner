using System.IO;
using IWshRuntimeLibrary;
using File = System.IO.File;

public static class StartupShortcutManager
{
    private static string ShortcutName => "GammaTuner.lnk";
    private static string StartupFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.Startup);
    private static string ShortcutPath => Path.Combine(StartupFolderPath, ShortcutName);

    public static void AddShortcutToStartup()
    {
        if (File.Exists(ShortcutPath))
            return;

        string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GammaTuner.exe");

        var shell = new WshShell();
        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(ShortcutPath);
        shortcut.TargetPath = exePath;
        shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
        shortcut.Description = "Launch GammaTuner at startup";
        shortcut.Save();
    }

    public static void RemoveShortcutFromStartup()
    {
        if (File.Exists(ShortcutPath))
            File.Delete(ShortcutPath);
    }

    public static bool IsShortcutInStartup()
    {
        return File.Exists(ShortcutPath);
    }
}
