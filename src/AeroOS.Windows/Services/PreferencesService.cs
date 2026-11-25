using System;
using System.IO;
using System.Runtime.Serialization.Json;
using AeroOS.Windows.Models;

namespace AeroOS.Windows.Services
{
    public sealed class PreferencesService
    {
        public string SettingsDirectory { get; private set; }
        public string SettingsPath { get; private set; }

        public PreferencesService()
        {
            SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AeroOSWindows");
            SettingsPath = Path.Combine(SettingsDirectory, "settings.json");
        }

        public AeroSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return AeroSettings.CreateDefault();
                using (var stream = File.OpenRead(SettingsPath))
                {
                    var serializer = new DataContractJsonSerializer(typeof(AeroSettings));
                    var settings = serializer.ReadObject(stream) as AeroSettings ?? AeroSettings.CreateDefault();
                    settings.Normalize();
                    return settings;
                }
            }
            catch
            {
                return AeroSettings.CreateDefault();
            }
        }

        public void Save(AeroSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            settings.Normalize();
            Directory.CreateDirectory(SettingsDirectory);
            var temporaryPath = SettingsPath + ".tmp";
            using (var stream = File.Create(temporaryPath))
            {
                var serializer = new DataContractJsonSerializer(typeof(AeroSettings));
                serializer.WriteObject(stream, settings);
                stream.Flush(true);
            }
            if (File.Exists(SettingsPath)) File.Replace(temporaryPath, SettingsPath, null);
            else File.Move(temporaryPath, SettingsPath);
        }
    }
}
