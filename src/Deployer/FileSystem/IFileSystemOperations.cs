using System.Threading.Tasks;

namespace Deployer.FileSystem
{
    public interface IFileSystemOperations
    {
        Task CopyDirectory(string sourceDir, string destinationDir);
        Task CopyFile(string source, string destination);
        Task DeleteDirectory(string path);
        Task DeleteFile(string path);
        Task<bool> DirectoryExists(string path);
        Task<bool> FileExists(string path);
        Task CreateDirectory(string path);
    }
}

