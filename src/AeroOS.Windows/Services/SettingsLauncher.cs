using System;
using System.Diagnostics;

namespace AeroOS.Windows.Services
{
    public static class SettingsLauncher
    {
        public static void Open(string settingsUri, string fallbackCommand)
        {
            try
            {
                Process.Start(new ProcessStartInfo(settingsUri) { UseShellExecute = true });
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(fallbackCommand))
                    Process.Start(new ProcessStartInfo("control.exe", fallbackCommand) { UseShellExecute = true });
                else
                    throw;
            }
        }
    }
}
