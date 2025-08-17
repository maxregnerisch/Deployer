using System.IO.Abstractions;
using Autofac;

using Zafiro.System.Windows;

namespace Deployer
{
    public class Windows23H2Module : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register Windows 23H2 image service
            builder.RegisterDecorator<Windows23H2ImageService, IWindowsImageService>();
            
            // Register Windows 23H2 boot creator
            builder.RegisterDecorator<Windows23H2BootCreator, IBootCreator>();
            
            // Logging service registration removed
            
            // Auto update service has been removed
            
            // Register device compatibility checker
            builder.RegisterType<DeviceCompatibilityChecker>()
                .AsSelf()
                .SingleInstance();
            
            // Register file system
            builder.RegisterType<FileSystem>()
                .As<IFileSystem>()
                .SingleInstance();
        }
    }
}
