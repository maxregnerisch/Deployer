using System.Threading.Tasks;

namespace Deployer
{
    public interface ISpaceAllocator
    {
        string SupportedDeviceType { get; }
        Task AllocateSpace(bool dualBoot);
    }
}

