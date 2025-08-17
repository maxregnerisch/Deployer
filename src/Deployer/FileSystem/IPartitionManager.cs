using System.Threading.Tasks;

namespace Deployer.FileSystem
{
    public interface IPartitionManager
    {
        Task CreatePartition(uint diskNumber, string name, ulong size, PartitionType partitionType);
        Task DeletePartition(uint diskNumber, uint partitionNumber);
        Task ShrinkPartition(uint diskNumber, uint partitionNumber, ulong sizeToShrink);
        Task ExtendPartition(uint diskNumber, uint partitionNumber, ulong sizeToExtend);
        Task AssignDriveLetter(uint diskNumber, uint partitionNumber, char driveLetter);
    }

    public enum PartitionType
    {
        Basic,
        Efi,
        Reserved,
        Recovery
    }
}

