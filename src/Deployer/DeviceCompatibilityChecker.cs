using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using Serilog;

namespace Deployer
{
    public class DeviceCompatibilityChecker
    {
        private static readonly Dictionary<string, DeviceCompatibilityInfo> KnownDevices = new Dictionary<string, DeviceCompatibilityInfo>(StringComparer.OrdinalIgnoreCase)
        {
            // Lumia devices
            { "Lumia 950", new DeviceCompatibilityInfo("Lumia 950", "RM-1104", true, "Fully compatible with Windows 11 23H2") },
            { "Lumia 950 XL", new DeviceCompatibilityInfo("Lumia 950 XL", "RM-1085", true, "Fully compatible with Windows 11 23H2") },
            
            // Surface devices
            { "Surface Pro 3", new DeviceCompatibilityInfo("Surface Pro 3", "Surface Pro 3", true, "Compatible with Windows 11 23H2 with some limitations") },
            { "Surface Pro 4", new DeviceCompatibilityInfo("Surface Pro 4", "Surface Pro 4", true, "Fully compatible with Windows 11 23H2") },
            { "Surface Pro 5", new DeviceCompatibilityInfo("Surface Pro 5", "Surface Pro 5", true, "Fully compatible with Windows 11 23H2") },
            { "Surface Pro 6", new DeviceCompatibilityInfo("Surface Pro 6", "Surface Pro 6", true, "Fully compatible with Windows 11 23H2") },
            { "Surface Pro 7", new DeviceCompatibilityInfo("Surface Pro 7", "Surface Pro 7", true, "Fully compatible with Windows 11 23H2") },
            { "Surface Pro 8", new DeviceCompatibilityInfo("Surface Pro 8", "Surface Pro 8", true, "Fully compatible with Windows 11 23H2") },
            { "Surface Pro 9", new DeviceCompatibilityInfo("Surface Pro 9", "Surface Pro 9", true, "Fully compatible with Windows 11 23H2") },
            
            // Other devices
            { "Raspberry Pi 4", new DeviceCompatibilityInfo("Raspberry Pi 4", "Raspberry Pi 4", true, "Compatible with Windows 11 23H2 with ARM64 build") },
            { "Generic PC", new DeviceCompatibilityInfo("Generic PC", "PC", true, "Compatibility depends on hardware specifications") },
        };

        public static async Task<DeviceCompatibilityInfo> CheckDeviceCompatibility()
        {
            try
            {
                var deviceInfo = await GetDeviceInfo();
                
                if (KnownDevices.TryGetValue(deviceInfo.Model, out var compatibilityInfo))
                {
                    return compatibilityInfo;
                }
                
                // For unknown devices, perform hardware compatibility check
                return await CheckHardwareCompatibility(deviceInfo);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking device compatibility");
                return new DeviceCompatibilityInfo("Unknown", "Unknown", false, "Could not determine device compatibility");
            }
        }

        private static async Task<DeviceInfo> GetDeviceInfo()
        {
            var deviceInfo = new DeviceInfo();
            
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        deviceInfo.Manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown";
                        deviceInfo.Model = obj["Model"]?.ToString() ?? "Unknown";
                        break;
                    }
                }
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        deviceInfo.ProcessorName = obj["Name"]?.ToString() ?? "Unknown";
                        deviceInfo.ProcessorCores = Convert.ToInt32(obj["NumberOfCores"]);
                        deviceInfo.ProcessorThreads = Convert.ToInt32(obj["NumberOfLogicalProcessors"]);
                        break;
                    }
                }
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    ulong totalMemory = 0;
                    foreach (var obj in searcher.Get())
                    {
                        totalMemory += Convert.ToUInt64(obj["Capacity"]);
                    }
                    deviceInfo.TotalMemoryGB = (int)(totalMemory / 1073741824); // Convert bytes to GB
                }
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        deviceInfo.DiskSizeGB = (int)(Convert.ToUInt64(obj["Size"]) / 1073741824); // Convert bytes to GB
                        break;
                    }
                }
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        deviceInfo.GraphicsCardName = obj["Name"]?.ToString() ?? "Unknown";
                        break;
                    }
                }
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_TPM"))
                {
                    deviceInfo.HasTPM = searcher.Get().Count > 0;
                }
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        deviceInfo.SecureBootEnabled = Convert.ToBoolean(obj["SecureBootEnabled"]);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting device information");
            }
            
            return deviceInfo;
        }

        private static async Task<DeviceCompatibilityInfo> CheckHardwareCompatibility(DeviceInfo deviceInfo)
        {
            // Windows 11 23H2 minimum requirements
            bool meetsMinimumRequirements = 
                deviceInfo.ProcessorCores >= 2 &&
                deviceInfo.TotalMemoryGB >= 4 &&
                deviceInfo.DiskSizeGB >= 64;
            
            // Windows 11 23H2 recommended requirements
            bool meetsRecommendedRequirements =
                deviceInfo.ProcessorCores >= 4 &&
                deviceInfo.ProcessorThreads >= 8 &&
                deviceInfo.TotalMemoryGB >= 8 &&
                deviceInfo.DiskSizeGB >= 128 &&
                deviceInfo.HasTPM &&
                deviceInfo.SecureBootEnabled;
            
            string compatibilityMessage;
            bool isCompatible;
            
            if (meetsRecommendedRequirements)
            {
                compatibilityMessage = "Fully compatible with Windows 11 23H2";
                isCompatible = true;
            }
            else if (meetsMinimumRequirements)
            {
                compatibilityMessage = "Compatible with Windows 11 23H2 with some limitations";
                isCompatible = true;
            }
            else
            {
                compatibilityMessage = "Device does not meet minimum requirements for Windows 11 23H2";
                isCompatible = false;
            }
            
            return new DeviceCompatibilityInfo(
                deviceInfo.Model,
                $"{deviceInfo.Manufacturer} {deviceInfo.Model}",
                isCompatible,
                compatibilityMessage);
        }
    }

    public class DeviceInfo
    {
        public string Manufacturer { get; set; } = "Unknown";
        public string Model { get; set; } = "Unknown";
        public string ProcessorName { get; set; } = "Unknown";
        public int ProcessorCores { get; set; }
        public int ProcessorThreads { get; set; }
        public int TotalMemoryGB { get; set; }
        public int DiskSizeGB { get; set; }
        public string GraphicsCardName { get; set; } = "Unknown";
        public bool HasTPM { get; set; }
        public bool SecureBootEnabled { get; set; }
    }

    public class DeviceCompatibilityInfo
    {
        public string DeviceName { get; }
        public string DeviceModel { get; }
        public bool IsCompatible { get; }
        public string CompatibilityMessage { get; }
        
        public DeviceCompatibilityInfo(string deviceName, string deviceModel, bool isCompatible, string compatibilityMessage)
        {
            DeviceName = deviceName;
            DeviceModel = deviceModel;
            IsCompatible = isCompatible;
            CompatibilityMessage = compatibilityMessage;
        }
    }
}

