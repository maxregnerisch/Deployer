namespace Deployer.FileSystem
{
    public class Partition
    {
        public uint Number { get; set; }
        public string Name { get; set; }
        public string Letter { get; set; }
        public ulong Size { get; set; }
        public string PartitionType { get; set; }
    }
}

