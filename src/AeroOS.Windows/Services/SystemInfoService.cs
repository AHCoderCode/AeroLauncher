using System;
using System.IO;
using System.Linq;
using System.Management;
using AeroOS.Windows.Models;

namespace AeroOS.Windows.Services
{
    public sealed class SystemInfoService
    {
        public SystemSnapshot ReadSnapshot()
        {
            var snapshot = new SystemSnapshot
            {
                OperatingSystem = Environment.OSVersion.VersionString,
                Architecture = Environment.Is64BitOperatingSystem ? "64-bit Windows" : "32-bit Windows",
                CpuName = "Not reported",
                LogicalProcessors = Environment.ProcessorCount,
                GpuName = "Not reported",
                TotalMemoryGb = 0,
                AvailableMemoryGb = 0,
                SystemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\",
                FreeSystemDriveGb = 0,
                TotalSystemDriveGb = 0
            };

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfLogicalProcessors FROM Win32_Processor"))
                using (var results = searcher.Get())
                {
                    var cpu = results.Cast<ManagementObject>().FirstOrDefault();
                    if (cpu != null)
                    {
                        snapshot.CpuName = Convert.ToString(cpu["Name"]);
                        if (cpu["NumberOfLogicalProcessors"] != null)
                            snapshot.LogicalProcessors = Convert.ToInt32(cpu["NumberOfLogicalProcessors"]);
                    }
                }
            }
            catch { }

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
                using (var results = searcher.Get())
                {
                    var os = results.Cast<ManagementObject>().FirstOrDefault();
                    if (os != null)
                    {
                        var totalKb = Convert.ToDouble(os["TotalVisibleMemorySize"]);
                        var freeKb = Convert.ToDouble(os["FreePhysicalMemory"]);
                        snapshot.TotalMemoryGb = Math.Round(totalKb / 1024d / 1024d, 1);
                        snapshot.AvailableMemoryGb = Math.Round(freeKb / 1024d / 1024d, 1);
                    }
                }
            }
            catch { }

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                using (var results = searcher.Get())
                {
                    snapshot.GpuName = string.Join("; ", results.Cast<ManagementObject>()
                        .Select(x => Convert.ToString(x["Name"]))
                        .Where(x => !string.IsNullOrWhiteSpace(x)).Take(2));
                    if (string.IsNullOrWhiteSpace(snapshot.GpuName)) snapshot.GpuName = "Not reported";
                }
            }
            catch { }

            try
            {
                var drive = new DriveInfo(snapshot.SystemDrive);
                if (drive.IsReady)
                {
                    snapshot.FreeSystemDriveGb = Math.Round(drive.AvailableFreeSpace / 1024d / 1024d / 1024d, 1);
                    snapshot.TotalSystemDriveGb = Math.Round(drive.TotalSize / 1024d / 1024d / 1024d, 1);
                }
            }
            catch { }

            return snapshot;
        }
    }
}
