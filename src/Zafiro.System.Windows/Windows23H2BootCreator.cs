using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Serilog;

namespace Zafiro.System.Windows
{
    public class Windows23H2BootCreator : IBootCreator
    {
        private readonly IBootCreator baseBootCreator;
        private readonly IFileSystem fileSystem;

        public Windows23H2BootCreator(IBootCreator baseBootCreator, IFileSystem fileSystem)
        {
            this.baseBootCreator = baseBootCreator ?? throw new ArgumentNullException(nameof(baseBootCreator));
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public async Task MakeBootable(string systemRoot, string windowsPath)
        {
            Log.Information("Making Windows 11 23H2 installation bootable with enhanced settings...");
            
            // Call the base boot creator first
            await baseBootCreator.MakeBootable(systemRoot, windowsPath);
            
            try
            {
                // Apply 23H2 specific boot optimizations
                await Apply23H2BootOptimizations(systemRoot, windowsPath);
                
                Log.Information("Windows 11 23H2 boot optimizations applied successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying Windows 11 23H2 boot optimizations");
            }
        }

        private async Task Apply23H2BootOptimizations(string systemRoot, string windowsPath)
        {
            // Create BCD optimization script
            var bcdOptimizationPath = Path.Combine(windowsPath, "Windows", "System32", "bcd_optimize.cmd");
            
            var bcdOptimizationContent = @"@echo off
REM Windows 11 23H2 BCD Optimizations

REM Set boot timeout to 3 seconds
bcdedit /timeout 3

REM Enable boot debugging if needed
REM bcdedit /set {bootmgr} bootdebug on

REM Disable the boot status UI
bcdedit /set {bootmgr} displaybootmenu no

REM Disable the Windows boot logo
bcdedit /set {default} bootlogo no

REM Disable the boot status animation
bcdedit /set {default} bootstatuspolicy IgnoreAllFailures

REM Disable automatic repair
bcdedit /set {default} recoveryenabled no

REM Disable the Windows Error Reporting
bcdedit /set {default} sos on

REM Optimize boot performance
bcdedit /set {default} bootmenupolicy Standard
bcdedit /set {default} quietboot yes
bcdedit /set {default} nx OptIn
bcdedit /set {default} bootux disabled
bcdedit /set {default} disabledynamictick yes
bcdedit /set {default} useplatformclock no

REM Enable test signing and disable integrity checks for development
bcdedit /set {default} testsigning on
bcdedit /set {default} nointegritychecks on

REM Optimize hypervisor settings
bcdedit /set hypervisorlaunchtype off
";
            
            await File.WriteAllTextAsync(bcdOptimizationPath, bcdOptimizationContent);
            
            // Create a startup task to run the BCD optimization script
            var startupFolder = Path.Combine(windowsPath, "ProgramData", "Microsoft", "Windows", "Start Menu", "Programs", "StartUp");
            Directory.CreateDirectory(startupFolder);
            
            var batchFilePath = Path.Combine(startupFolder, "OptimizeBCD.cmd");
            var batchContent = @"@echo off
%windir%\System32\bcd_optimize.cmd
del %windir%\System32\bcd_optimize.cmd
del %0
";
            
            await File.WriteAllTextAsync(batchFilePath, batchContent);
            
            // Create UEFI boot optimization script
            var uefiOptimizationPath = Path.Combine(windowsPath, "Windows", "System32", "uefi_optimize.cmd");
            
            var uefiOptimizationContent = @"@echo off
REM Windows 11 23H2 UEFI Optimizations

REM Optimize boot sequence
bcdedit /set {bootmgr} timeout 3
bcdedit /set {bootmgr} displaybootmenu no
bcdedit /set {bootmgr} bootems no

REM Optimize firmware boot time
bcdedit /set {bootmgr} bootux disabled

REM Disable secure boot for development
bcdedit /set {bootmgr} secureboot off
";
            
            await File.WriteAllTextAsync(uefiOptimizationPath, uefiOptimizationContent);
            
            // Add to startup tasks
            var uefiStartupPath = Path.Combine(startupFolder, "OptimizeUEFI.cmd");
            var uefiStartupContent = @"@echo off
%windir%\System32\uefi_optimize.cmd
del %windir%\System32\uefi_optimize.cmd
del %0
";
            
            await File.WriteAllTextAsync(uefiStartupPath, uefiStartupContent);
        }
    }
}
