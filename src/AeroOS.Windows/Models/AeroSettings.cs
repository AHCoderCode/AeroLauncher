using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AeroOS.Windows.Models
{
    [DataContract]
    public sealed class AeroSettings
    {
        [DataMember] public string Theme { get; set; }
        [DataMember] public string AccentColor { get; set; }
        [DataMember] public bool PerformanceMode { get; set; }
        [DataMember] public string WallpaperPath { get; set; }
        [DataMember] public List<string> PinnedPaths { get; set; }
        [DataMember] public List<string> RecentPaths { get; set; }

        public static AeroSettings CreateDefault()
        {
            return new AeroSettings
            {
                Theme = "Dark",
                AccentColor = "#8B7CF6",
                PerformanceMode = true,
                WallpaperPath = string.Empty,
                PinnedPaths = new List<string>(),
                RecentPaths = new List<string>()
            };
        }

        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(Theme)) Theme = "Dark";
            if (string.IsNullOrWhiteSpace(AccentColor)) AccentColor = "#8B7CF6";
            if (PinnedPaths == null) PinnedPaths = new List<string>();
            if (RecentPaths == null) RecentPaths = new List<string>();
            if (WallpaperPath == null) WallpaperPath = string.Empty;
        }
    }
}
