using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AeroOS.Windows.Models;

namespace AeroOS.Windows.Services
{
    public sealed class ApplicationCatalog
    {
        private const int MaximumShortcuts = 1200;
        private const int MaximumFolderDepth = 8;

        public List<AppEntry> Discover()
        {
            var results = new Dictionary<string, AppEntry>(StringComparer.OrdinalIgnoreCase);
            var pending = new Stack<KeyValuePair<string, int>>();
            var folders = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
                Environment.GetFolderPath(Environment.SpecialFolder.Programs)
            };

            foreach (var folder in folders.Where(path => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path)))
                pending.Push(new KeyValuePair<string, int>(folder, 0));

            while (pending.Count > 0 && results.Count < MaximumShortcuts)
            {
                var current = pending.Pop();
                try
                {
                    foreach (var file in Directory.EnumerateFiles(current.Key, "*.lnk", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            var name = Path.GetFileNameWithoutExtension(file);
                            if (string.IsNullOrWhiteSpace(name) || IsNoise(name)) continue;
                            if (!results.ContainsKey(name))
                                results.Add(name, new AppEntry { Name = name, Path = file, Category = GuessCategory(file) });
                            if (results.Count >= MaximumShortcuts) break;
                        }
                        catch { }
                    }
                }
                catch { }

                if (current.Value >= MaximumFolderDepth) continue;
                try
                {
                    foreach (var child in Directory.EnumerateDirectories(current.Key, "*", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            if ((File.GetAttributes(child) & FileAttributes.ReparsePoint) != 0) continue;
                            pending.Push(new KeyValuePair<string, int>(child, current.Value + 1));
                        }
                        catch { }
                    }
                }
                catch { }
            }

            return results.Values.OrderBy(app => app.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
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