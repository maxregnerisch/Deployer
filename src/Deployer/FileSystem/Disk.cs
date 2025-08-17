using System.Collections.Generic;

namespace Deployer.FileSystem
{
    public class Disk
    {
        public uint Number { get; set; }
        public ulong Size { get; set; }
        public ulong AllocatedSize { get; set; }
        public string FriendlyName { get; set; }
        public bool IsSystem { get; set; }
        public bool IsOffline { get; set; }
        public IList<Partition> Partitions { get; set; } = new List<Partition>();
    }
}

