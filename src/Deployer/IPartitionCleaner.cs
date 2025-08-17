using System.Threading.Tasks;

namespace Deployer
{
    public interface IPartitionCleaner
    {
        Task Clean();
    }
}

