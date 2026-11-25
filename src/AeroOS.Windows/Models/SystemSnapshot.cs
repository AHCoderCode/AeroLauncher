namespace AeroOS.Windows.Models
{
    public sealed class SystemSnapshot
    {
        public string OperatingSystem { get; set; }
        public string Architecture { get; set; }
        public string CpuName { get; set; }
        public int LogicalProcessors { get; set; }
        public string GpuName { get; set; }
        public double TotalMemoryGb { get; set; }
        public double AvailableMemoryGb { get; set; }
        public string SystemDrive { get; set; }
        public double FreeSystemDriveGb { get; set; }
        public double TotalSystemDriveGb { get; set; }
    }
}
