using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deployer
{
    public abstract class Device
    {
        public abstract string Name { get; }
        public abstract string Model { get; }
        public abstract string DeviceType { get; }
        public abstract bool SupportsWim { get; }
        public abstract bool SupportsDualBoot { get; }

        public abstract Task<IList<DriverMetadata>> GetDrivers();
        public abstract Task<DiskInfo> GetDiskInfo();
        public abstract Task<bool> IsWoaPresent();
        public abstract Task<bool> IsOobeCompleted();
        public abstract Task ClearWindows();
        public abstract Task<bool> PrepareForDeployment(bool dualBoot);
        public abstract Task<string> GetWindowsVolumeLetter();
        public abstract Task<string> GetSystemVolumeLetter();
    }

    public class DriverMetadata
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class DiskInfo
    {
        public uint Number { get; set; }
        public ulong Size { get; set; }
        public ulong AllocatedSize { get; set; }
        public string FriendlyName { get; set; }
        public bool IsSystem { get; set; }
        public bool IsOffline { get; set; }
        public IList<PartitionInfo> Partitions { get; set; }
    }

    public class PartitionInfo
    {
        public uint Number { get; set; }
        public string Name { get; set; }
        public string Letter { get; set; }
        public ulong Size { get; set; }
        public string PartitionType { get; set; }
    }
}

