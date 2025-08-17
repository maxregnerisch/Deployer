using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Zafiro.Core;
using Zafiro.Storage;

namespace Zafiro.System.Windows
{
    public class Windows23H2ImageService : IWindowsImageService
    {
        private readonly IWindowsImageService baseImageService;
        private readonly IFileSystem fileSystem;

        public Windows23H2ImageService(IWindowsImageService baseImageService, IFileSystem fileSystem)
        {
            this.baseImageService = baseImageService ?? throw new ArgumentNullException(nameof(baseImageService));
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public async Task ApplyImage(IPartition target, string imagePath, int imageIndex = 1, bool useCompact = false,
            IOperationProgress progressObserver = null, CancellationToken token = default)
        {
            await baseImageService.ApplyImage(target, imagePath, imageIndex, useCompact, progressObserver, token);
            await Apply23H2Optimizations(target.Root);
        }

        public Task<IList<string>> InjectDrivers(string path, string windowsRootPath)
        {
            return baseImageService.InjectDrivers(path, windowsRootPath);
        }

        public Task RemoveDriver(string path, string windowsRootPath)
        {
            return baseImageService.RemoveDriver(path, windowsRootPath);
        }

        public Task CaptureImage(IPartition source, string destination, IOperationProgress progressObserver = null,
            CancellationToken cancellationToken = default)
        {
            return baseImageService.CaptureImage(source, destination, progressObserver, cancellationToken);
        }

        public async Task ApplyImage(string targetDriveRoot, string imagePath, int imageIndex = 1, bool useCompact = false,
            IOperationProgress progressObserver = null, CancellationToken token = default)
        {
            await baseImageService.ApplyImage(targetDriveRoot, imagePath, imageIndex, useCompact, progressObserver, token);
            await Apply23H2Optimizations(targetDriveRoot);
        }

        private async Task Apply23H2Optimizations(string targetDriveRoot)
        {
            Log.Information("Applying Windows 11 23H2 optimizations...");
            
            try
            {
                // Create registry optimization files
                await CreateRegistryOptimizations(targetDriveRoot);
                
                // Disable memory compression for better performance
                await DisableMemoryCompression(targetDriveRoot);
                
                // Optimize boot configuration
                await OptimizeBootConfiguration(targetDriveRoot);
                
                Log.Information("Windows 11 23H2 optimizations applied successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying Windows 11 23H2 optimizations");
            }
        }

        private async Task CreateRegistryOptimizations(string targetDriveRoot)
        {
            var regFilePath = Path.Combine(targetDriveRoot, "Windows", "System32", "23h2_optimizations.reg");
            
            var regContent = @"Windows Registry Editor Version 5.00

; Windows 11 23H2 Optimizations

; Improve system responsiveness
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\PriorityControl]
""Win32PrioritySeparation""=dword:00000026

; Disable telemetry
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection]
""AllowTelemetry""=dword:00000000

; Optimize for performance
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects]
""VisualFXSetting""=dword:00000002

; Disable startup delay
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize]
""StartupDelayInMSec""=dword:00000000

; Optimize network settings
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters]
""DefaultTTL""=dword:00000080
""Tcp1323Opts""=dword:00000001
""TcpMaxDupAcks""=dword:00000002
""SackOpts""=dword:00000001

; Improve SSD performance
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management]
""EnablePrefetcher""=dword:00000000
""EnableSuperfetch""=dword:00000000
";

            await File.WriteAllTextAsync(regFilePath, regContent);
            
            // Create a startup task to import the registry file
            var startupFolder = Path.Combine(targetDriveRoot, "ProgramData", "Microsoft", "Windows", "Start Menu", "Programs", "StartUp");
            Directory.CreateDirectory(startupFolder);
            
            var batchFilePath = Path.Combine(startupFolder, "Apply23H2Optimizations.cmd");
            var batchContent = @"@echo off
reg import %windir%\System32\23h2_optimizations.reg
del %windir%\System32\23h2_optimizations.reg
del %0
";
            
            await File.WriteAllTextAsync(batchFilePath, batchContent);
        }

        private async Task DisableMemoryCompression(string targetDriveRoot)
        {
            var scriptPath = Path.Combine(targetDriveRoot, "Windows", "System32", "disable_memory_compression.ps1");
            
            var scriptContent = @"
# Disable memory compression for better performance
Disable-MMAgent -MemoryCompression

# Optimize virtual memory settings
$computersys = Get-WmiObject Win32_ComputerSystem -EnableAllPrivileges
$computersys.AutomaticManagedPagefile = $False
$computersys.Put()

$pagefile = Get-WmiObject -Query 'Select * From Win32_PageFileSetting Where Name = ""C:\\pagefile.sys""'
$pagefile.InitialSize = 8192
$pagefile.MaximumSize = 16384
$pagefile.Put()
";
            
            await File.WriteAllTextAsync(scriptPath, scriptContent);
            
            // Create a startup task to run the PowerShell script
            var startupFolder = Path.Combine(targetDriveRoot, "ProgramData", "Microsoft", "Windows", "Start Menu", "Programs", "StartUp");
            Directory.CreateDirectory(startupFolder);
            
            var batchFilePath = Path.Combine(startupFolder, "DisableMemoryCompression.cmd");
            var batchContent = @"@echo off
powershell -ExecutionPolicy Bypass -File %windir%\System32\disable_memory_compression.ps1
del %windir%\System32\disable_memory_compression.ps1
del %0
";
            
            await File.WriteAllTextAsync(batchFilePath, batchContent);
        }

        private async Task OptimizeBootConfiguration(string targetDriveRoot)
        {
            // Create a boot configuration optimization script
            var scriptPath = Path.Combine(targetDriveRoot, "Windows", "System32", "optimize_boot.cmd");
            
            var scriptContent = @"@echo off
bcdedit /set {current} bootmenupolicy Standard
bcdedit /set {current} quietboot yes
bcdedit /set {current} bootlog no
bcdedit /set {current} nx OptIn
bcdedit /set {current} bootux disabled
bcdedit /set {current} disabledynamictick yes
bcdedit /set {current} useplatformclock no
bcdedit /timeout 3
";
            
            await File.WriteAllTextAsync(scriptPath, scriptContent);
            
            // Create a startup task to run the boot optimization script
            var startupFolder = Path.Combine(targetDriveRoot, "ProgramData", "Microsoft", "Windows", "Start Menu", "Programs", "StartUp");
            Directory.CreateDirectory(startupFolder);
            
            var batchFilePath = Path.Combine(startupFolder, "OptimizeBoot.cmd");
            var batchContent = @"@echo off
%windir%\System32\optimize_boot.cmd
del %windir%\System32\optimize_boot.cmd
del %0
";
            
            await File.WriteAllTextAsync(batchFilePath, batchContent);
        }
    }
}

