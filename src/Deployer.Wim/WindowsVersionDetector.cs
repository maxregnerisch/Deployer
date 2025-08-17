using System;
using System.Collections.Generic;
using System.Linq;

namespace Deployer.Wim
{
    public class WindowsVersionDetector
    {
        private static readonly Dictionary<string, WindowsVersionInfo> KnownVersions = new Dictionary<string, WindowsVersionInfo>
        {
            // Windows 10 versions
            { "10.0.19041", new WindowsVersionInfo("Windows 10", "2004", "May 2020 Update") },
            { "10.0.19042", new WindowsVersionInfo("Windows 10", "20H2", "October 2020 Update") },
            { "10.0.19043", new WindowsVersionInfo("Windows 10", "21H1", "May 2021 Update") },
            { "10.0.19044", new WindowsVersionInfo("Windows 10", "21H2", "November 2021 Update") },
            { "10.0.19045", new WindowsVersionInfo("Windows 10", "22H2", "2022 Update") },
            
            // Windows 11 versions
            { "10.0.22000", new WindowsVersionInfo("Windows 11", "21H2", "Initial Release") },
            { "10.0.22621", new WindowsVersionInfo("Windows 11", "22H2", "2022 Update") },
            { "10.0.22631", new WindowsVersionInfo("Windows 11", "23H2", "2023 Update") },
            
            // Future Windows 11 versions (placeholder)
            { "10.0.23000", new WindowsVersionInfo("Windows 11", "24H1", "2024 Update") },
            { "10.0.23500", new WindowsVersionInfo("Windows 11", "24H2", "2024 Update") },
        };

        public static WindowsVersionInfo DetectVersion(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            string versionKey = $"{version.Major}.{version.Minor}.{version.Build}";
            
            if (KnownVersions.TryGetValue(versionKey, out var versionInfo))
            {
                return versionInfo;
            }

            // Try to determine Windows version based on build number
            int buildNumber = int.Parse(version.Build);
            
            if (buildNumber >= 22000)
            {
                return new WindowsVersionInfo("Windows 11", "Unknown", $"Build {version.Build}");
            }
            
            if (buildNumber >= 10240)
            {
                return new WindowsVersionInfo("Windows 10", "Unknown", $"Build {version.Build}");
            }

            return new WindowsVersionInfo("Unknown Windows", "Unknown", $"Build {version.Build}");
        }

        public static bool Is23H2OrNewer(Version version)
        {
            if (version == null)
            {
                return false;
            }

            // Windows 11 23H2 is build 22631 or higher
            return version.Major == "10" && version.Minor == "0" && int.Parse(version.Build) >= 22631;
        }
    }

    public class WindowsVersionInfo
    {
        public string Name { get; }
        public string ReleaseId { get; }
        public string DisplayName { get; }
        
        public WindowsVersionInfo(string name, string releaseId, string displayName)
        {
            Name = name;
            ReleaseId = releaseId;
            DisplayName = displayName;
        }

        public override string ToString()
        {
            return $"{Name} {ReleaseId} ({DisplayName})";
        }
    }
}

