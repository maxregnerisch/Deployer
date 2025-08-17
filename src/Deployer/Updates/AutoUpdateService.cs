using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Octokit;
using Serilog;

namespace Deployer.Updates
{
    public class AutoUpdateService
    {
        private static AutoUpdateService _instance;
        private readonly HttpClient _httpClient;
        private readonly GitHubClient _gitHubClient;
        private readonly string _owner = "maxregnerisch";
        private readonly string _repo = "Deployer";
        private readonly string _currentVersion;
        
        public static AutoUpdateService Instance => _instance ??= new AutoUpdateService();
        
        private AutoUpdateService()
        {
            _httpClient = new HttpClient();
            _gitHubClient = new GitHubClient(new ProductHeaderValue("Deployer"));
            
            // Get current version from assembly
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            _currentVersion = version?.ToString() ?? "1.0.0.0";
        }
        
        public string CurrentVersion => _currentVersion;
        
        public event EventHandler<UpdateInfo> UpdateAvailable;
        
        public async Task<UpdateInfo> CheckForUpdates(bool includeBeta = false)
        {
            try
            {
                Log.Information("Checking for updates...");
                
                // Get latest release from GitHub
                var releases = await _gitHubClient.Repository.Release.GetAll(_owner, _repo);
                
                if (releases.Count == 0)
                {
                    Log.Information("No releases found");
                    return null;
                }
                
                // Find the latest release (non-beta unless includeBeta is true)
                Release latestRelease = null;
                
                foreach (var release in releases)
                {
                    if (!includeBeta && release.Prerelease)
                    {
                        continue;
                    }
                    
                    latestRelease = release;
                    break;
                }
                
                if (latestRelease == null)
                {
                    Log.Information("No suitable releases found");
                    return null;
                }
                
                // Parse version from tag name (remove 'v' prefix if present)
                var tagVersion = latestRelease.TagName;
                if (tagVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                {
                    tagVersion = tagVersion.Substring(1);
                }
                
                // Compare versions
                var currentVersionParts = _currentVersion.Split('.');
                var latestVersionParts = tagVersion.Split('.');
                
                bool isNewer = false;
                
                // Compare major version
                if (int.TryParse(latestVersionParts[0], out var latestMajor) && 
                    int.TryParse(currentVersionParts[0], out var currentMajor))
                {
                    if (latestMajor > currentMajor)
                    {
                        isNewer = true;
                    }
                    else if (latestMajor == currentMajor && latestVersionParts.Length > 1 && currentVersionParts.Length > 1)
                    {
                        // Compare minor version
                        if (int.TryParse(latestVersionParts[1], out var latestMinor) && 
                            int.TryParse(currentVersionParts[1], out var currentMinor))
                        {
                            if (latestMinor > currentMinor)
                            {
                                isNewer = true;
                            }
                            else if (latestMinor == currentMinor && latestVersionParts.Length > 2 && currentVersionParts.Length > 2)
                            {
                                // Compare patch version
                                if (int.TryParse(latestVersionParts[2], out var latestPatch) && 
                                    int.TryParse(currentVersionParts[2], out var currentPatch))
                                {
                                    if (latestPatch > currentPatch)
                                    {
                                        isNewer = true;
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (!isNewer)
                {
                    Log.Information("No updates available");
                    return null;
                }
                
                // Find installer asset
                string installerUrl = null;
                string zipUrl = null;
                
                foreach (var asset in latestRelease.Assets)
                {
                    if (asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && 
                        asset.Name.Contains("Setup", StringComparison.OrdinalIgnoreCase))
                    {
                        installerUrl = asset.BrowserDownloadUrl;
                    }
                    else if (asset.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        zipUrl = asset.BrowserDownloadUrl;
                    }
                }
                
                if (string.IsNullOrEmpty(installerUrl) && string.IsNullOrEmpty(zipUrl))
                {
                    Log.Warning("Update available but no download assets found");
                    return null;
                }
                
                var updateInfo = new UpdateInfo
                {
                    CurrentVersion = _currentVersion,
                    NewVersion = tagVersion,
                    ReleaseNotes = latestRelease.Body,
                    InstallerUrl = installerUrl,
                    ZipUrl = zipUrl,
                    IsBeta = latestRelease.Prerelease,
                    PublishedAt = latestRelease.PublishedAt.DateTime
                };
                
                Log.Information($"Update available: {updateInfo.NewVersion}");
                
                // Trigger event
                UpdateAvailable?.Invoke(this, updateInfo);
                
                return updateInfo;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking for updates");
                return null;
            }
        }
        
        public async Task<bool> DownloadAndInstallUpdate(UpdateInfo updateInfo)
        {
            try
            {
                if (updateInfo == null)
                {
                    Log.Error("No update information provided");
                    return false;
                }
                
                Log.Information($"Downloading update {updateInfo.NewVersion}...");
                
                // Prefer installer if available
                string downloadUrl = !string.IsNullOrEmpty(updateInfo.InstallerUrl) 
                    ? updateInfo.InstallerUrl 
                    : updateInfo.ZipUrl;
                
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    Log.Error("No download URL available");
                    return false;
                }
                
                // Create temp directory for download
                var tempDir = Path.Combine(Path.GetTempPath(), "DeployerUpdate");
                Directory.CreateDirectory(tempDir);
                
                // Determine file extension
                string fileExtension = downloadUrl.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? ".exe" : ".zip";
                var downloadPath = Path.Combine(tempDir, $"DeployerUpdate{fileExtension}");
                
                // Download the file
                using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    
                    using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var downloadStream = await response.Content.ReadAsStreamAsync())
                    {
                        await downloadStream.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                    }
                }
                
                Log.Information($"Update downloaded to {downloadPath}");
                
                // If it's an installer, run it
                if (fileExtension == ".exe")
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadPath,
                        UseShellExecute = true
                    });
                    
                    return true;
                }
                
                // If it's a ZIP, extract it
                // This is a simplified implementation - in a real app, you'd want to handle this more robustly
                Log.Information("ZIP updates not fully implemented yet");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error downloading and installing update");
                return false;
            }
        }
    }
    
    public class UpdateInfo
    {
        public string CurrentVersion { get; set; }
        public string NewVersion { get; set; }
        public string ReleaseNotes { get; set; }
        public string InstallerUrl { get; set; }
        public string ZipUrl { get; set; }
        public bool IsBeta { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}

