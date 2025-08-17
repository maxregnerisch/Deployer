using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deployer.FileSystem
{
    public interface IDiskApi
    {
        Task<IList<Disk>> GetDisks();
        Task<Disk> GetDisk(uint diskNumber);
        Task RefreshDisk(uint diskNumber);
    }
}

