using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AeroOS.Windows.Models;

namespace AeroOS.Windows.Services
{
    public sealed class ApplicationCatalog
    {
        public List<AppEntry> Discover()
        {
            var results = new Dictionary<string, AppEntry>(StringComparer.OrdinalIgnoreCase);
            var folders = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
                Environment.GetFolderPath(Environment.SpecialFolder.Programs)
            };

            foreach (var folder in folders.Where(Directory.Exists))
            {
                IEnumerable<string> files;
                try { files = Directory.EnumerateFiles(folder, "*.lnk", SearchOption.AllDirectories).ToArray(); }
                catch { continue; }

                foreach (var file in files)
                {
                    try
                    {
                        var name = Path.GetFileNameWithoutExtension(file);
                        if (string.IsNullOrWhiteSpace(name) || IsNoise(name)) continue;
                        if (!results.ContainsKey(name))
                        {
                            results.Add(name, new AppEntry
                            {
                                Name = name,
                                Path = file,
                                Category = GuessCategory(file)
                            });
                        }
                    }
                    catch { }
                }
            }

            return results.Values.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        private static bool IsNoise(string name)
        {
            var value = name.ToLowerInvariant();
            return value.Contains("uninstall") || value == "readme" || value == "help" || value == "website";
        }

        private static string GuessCategory(string shortcutPath)
        {
            var value = shortcutPath.ToLowerInvariant();
            if (value.Contains("accessories")) return "Accessories";
            if (value.Contains("system tools")) return "System tools";
            if (value.Contains("startup")) return "Startup";
            if (value.Contains("games")) return "Games";
            return "Windows application";
        }
    }
}
