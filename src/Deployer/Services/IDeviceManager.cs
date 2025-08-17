using System.Threading.Tasks;

namespace Deployer.Services
{
    public interface IDeviceManager
    {
        Task<bool> EnableDevice(string deviceId);
        Task<bool> DisableDevice(string deviceId);
    }
}

