using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using Deployer.Logging;
using Deployer.Updates;
using ReactiveUI;
using Serilog;

namespace Deployer.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _statusText = "Ready";
        private double _progress = 0;
        private bool _isIndeterminate = false;
        private bool _isBusy = false;
        private bool _isDarkMode = false;
        private string _selectedDevice;
        private string _imagePath;
        private bool _useCompactOs = true;
        private bool _optimizePerformance = true;
        private bool _disableTelemetry = true;
        private bool _enableDarkMode = false;
        private bool _useCustomPartitioning = false;
        private bool _injectDrivers = false;
        private bool _enableBootDebugging = false;
        private bool _disableSecureBoot = false;
        private string _logContent = "";
        private string _deviceName = "Unknown";
        private string _manufacturer = "Unknown";
        private string _processor = "Unknown";
        private string _memory = "Unknown";
        private string _storage = "Unknown";
        private string _graphics = "Unknown";
        private string _compatibilityStatus = "Checking compatibility...";
        private string _compatibilityDetails = "Please wait...";
        private bool _isCompatible = false;
        private string _version;
        
        public MainWindowViewModel()
        {
            // Initialize commands
            DeployCommand = ReactiveCommand.CreateFromTask(DeployAsync);
            CancelCommand = ReactiveCommand.Create(Cancel);
            BrowseImageCommand = ReactiveCommand.Create(BrowseImage);
            RefreshDeviceInfoCommand = ReactiveCommand.CreateFromTask(RefreshDeviceInfoAsync);
            ClearLogsCommand = ReactiveCommand.Create(ClearLogs);
            ExportLogsCommand = ReactiveCommand.CreateFromTask(ExportLogsAsync);
            CheckForUpdatesCommand = ReactiveCommand.CreateFromTask(CheckForUpdatesAsync);
            VisitGitHubCommand = ReactiveCommand.Create(VisitGitHub);
            ToggleThemeCommand = ReactiveCommand.Create(ToggleTheme);
            
            // Initialize devices collection
            Devices = new ObservableCollection<string>
            {
                "Lumia 950",
                "Lumia 950 XL",
                "Surface Pro 3",
                "Surface Pro 4",
                "Surface Pro 5",
                "Surface Pro 6",
                "Surface Pro 7",
                "Surface Pro 8",
                "Surface Pro 9",
                "Raspberry Pi 4",
                "Generic PC"
            };
            
            // Set default selected device
            SelectedDevice = "Generic PC";
            
            // Set version
            Version = AutoUpdateService.Instance.CurrentVersion;
            
            // Initialize
            Initialize();
        }
        
        private async void Initialize()
        {
            try
            {
                // Refresh logs
                await RefreshLogsAsync();
                
                // Check for updates
                await CheckForUpdatesAsync(false);
                
                // Check device compatibility
                await RefreshDeviceInfoAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing view model");
            }
        }
        
        #region Properties
        
        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }
        
        public double Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }
        
        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set => this.RaiseAndSetIfChanged(ref _isIndeterminate, value);
        }
        
        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }
        
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set => this.RaiseAndSetIfChanged(ref _isDarkMode, value);
        }
        
        public ObservableCollection<string> Devices { get; }
        
        public string SelectedDevice
        {
            get => _selectedDevice;
            set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
        }
        
        public string ImagePath
        {
            get => _imagePath;
            set => this.RaiseAndSetIfChanged(ref _imagePath, value);
        }
        
        public bool UseCompactOs
        {
            get => _useCompactOs;
            set => this.RaiseAndSetIfChanged(ref _useCompactOs, value);
        }
        
        public bool OptimizePerformance
        {
            get => _optimizePerformance;
            set => this.RaiseAndSetIfChanged(ref _optimizePerformance, value);
        }
        
        public bool DisableTelemetry
        {
            get => _disableTelemetry;
            set => this.RaiseAndSetIfChanged(ref _disableTelemetry, value);
        }
        
        public bool EnableDarkMode
        {
            get => _enableDarkMode;
            set => this.RaiseAndSetIfChanged(ref _enableDarkMode, value);
        }
        
        public bool UseCustomPartitioning
        {
            get => _useCustomPartitioning;
            set => this.RaiseAndSetIfChanged(ref _useCustomPartitioning, value);
        }
        
        public bool InjectDrivers
        {
            get => _injectDrivers;
            set => this.RaiseAndSetIfChanged(ref _injectDrivers, value);
        }
        
        public bool EnableBootDebugging
        {
            get => _enableBootDebugging;
            set => this.RaiseAndSetIfChanged(ref _enableBootDebugging, value);
        }
        
        public bool DisableSecureBoot
        {
            get => _disableSecureBoot;
            set => this.RaiseAndSetIfChanged(ref _disableSecureBoot, value);
        }
        
        public string LogContent
        {
            get => _logContent;
            set => this.RaiseAndSetIfChanged(ref _logContent, value);
        }
        
        public string DeviceName
        {
            get => _deviceName;
            set => this.RaiseAndSetIfChanged(ref _deviceName, value);
        }
        
        public string Manufacturer
        {
            get => _manufacturer;
            set => this.RaiseAndSetIfChanged(ref _manufacturer, value);
        }
        
        public string Processor
        {
            get => _processor;
            set => this.RaiseAndSetIfChanged(ref _processor, value);
        }
        
        public string Memory
        {
            get => _memory;
            set => this.RaiseAndSetIfChanged(ref _memory, value);
        }
        
        public string Storage
        {
            get => _storage;
            set => this.RaiseAndSetIfChanged(ref _storage, value);
        }
        
        public string Graphics
        {
            get => _graphics;
            set => this.RaiseAndSetIfChanged(ref _graphics, value);
        }
        
        public string CompatibilityStatus
        {
            get => _compatibilityStatus;
            set => this.RaiseAndSetIfChanged(ref _compatibilityStatus, value);
        }
        
        public string CompatibilityDetails
        {
            get => _compatibilityDetails;
            set => this.RaiseAndSetIfChanged(ref _compatibilityDetails, value);
        }
        
        public bool IsCompatible
        {
            get => _isCompatible;
            set => this.RaiseAndSetIfChanged(ref _isCompatible, value);
        }
        
        public string Version
        {
            get => _version;
            set => this.RaiseAndSetIfChanged(ref _version, value);
        }
        
        #endregion
        
        #region Commands
        
        public ICommand DeployCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseImageCommand { get; }
        public ICommand RefreshDeviceInfoCommand { get; }
        public ICommand ClearLogsCommand { get; }
        public ICommand ExportLogsCommand { get; }
        public ICommand CheckForUpdatesCommand { get; }
        public ICommand VisitGitHubCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        
        #endregion
        
        #region Command Implementations
        
        private async Task DeployAsync()
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(ImagePath))
                {
                    StatusText = "Please select a Windows image";
                    return;
                }
                
                if (string.IsNullOrEmpty(SelectedDevice))
                {
                    StatusText = "Please select a target device";
                    return;
                }
                
                // Set busy state
                IsBusy = true;
                IsIndeterminate = true;
                StatusText = "Starting deployment...";
                
                // Log deployment start
                Log.Information($"Starting deployment to {SelectedDevice} with image {ImagePath}");
                Log.Information($"Options: CompactOS={UseCompactOs}, OptimizePerformance={OptimizePerformance}, DisableTelemetry={DisableTelemetry}, EnableDarkMode={EnableDarkMode}");
                Log.Information($"Advanced options: CustomPartitioning={UseCustomPartitioning}, InjectDrivers={InjectDrivers}, BootDebugging={EnableBootDebugging}, DisableSecureBoot={DisableSecureBoot}");
                
                // Simulate deployment progress
                await SimulateDeploymentProgressAsync();
                
                // Update logs
                await RefreshLogsAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during deployment");
                StatusText = $"Deployment failed: {ex.Message}";
                Progress = 0;
                IsIndeterminate = false;
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private async Task SimulateDeploymentProgressAsync()
        {
            IsIndeterminate = false;
            
            for (int i = 0; i <= 100; i += 5)
            {
                await Task.Delay(500);
                
                if (!IsBusy)
                {
                    StatusText = "Deployment cancelled";
                    Progress = 0;
                    Log.Information("Deployment cancelled");
                    break;
                }
                
                Progress = i;
                StatusText = $"Deploying Windows 11 23H2... {i}%";
                Log.Information($"Deployment progress: {i}%");
            }
            
            if (IsBusy)
            {
                StatusText = "Deployment completed successfully";
                Progress = 100;
                Log.Information("Deployment completed successfully");
            }
            
            IsBusy = false;
        }
        
        private void Cancel()
        {
            if (IsBusy)
            {
                IsBusy = false;
                StatusText = "Deployment cancelled";
                Progress = 0;
                IsIndeterminate = false;
                Log.Information("Deployment cancelled");
            }
        }
        
        private void BrowseImage()
        {
            // TODO: Implement file dialog
            ImagePath = "C:\\Windows11_23H2.wim";
            Log.Information($"Selected image: {ImagePath}");
        }
        
        private async Task RefreshDeviceInfoAsync()
        {
            try
            {
                // Update UI to show checking status
                CompatibilityStatus = "Checking compatibility...";
                CompatibilityDetails = "Please wait...";
                IsCompatible = false;
                
                // Check device compatibility
                var compatibilityInfo = await DeviceCompatibilityChecker.CheckDeviceCompatibility();
                
                if (compatibilityInfo == null)
                {
                    CompatibilityStatus = "Compatibility check failed";
                    CompatibilityDetails = "Could not determine device compatibility";
                    IsCompatible = false;
                    return;
                }
                
                // Update UI with compatibility info
                CompatibilityStatus = compatibilityInfo.IsCompatible 
                    ? "Compatible with Windows 11 23H2" 
                    : "Not compatible with Windows 11 23H2";
                
                CompatibilityDetails = compatibilityInfo.CompatibilityMessage;
                IsCompatible = compatibilityInfo.IsCompatible;
                
                // Update device info
                DeviceName = compatibilityInfo.DeviceName;
                
                // Log result
                Log.Information($"Device compatibility check: {CompatibilityStatus}");
                Log.Information($"Compatibility details: {CompatibilityDetails}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking device compatibility");
                CompatibilityStatus = "Compatibility check failed";
                CompatibilityDetails = $"Error: {ex.Message}";
                IsCompatible = false;
            }
        }
        
        private void ClearLogs()
        {
            LoggingService.Instance.ClearLogs();
            RefreshLogsAsync().ConfigureAwait(false);
            Log.Information("Logs cleared");
        }
        
        private async Task ExportLogsAsync()
        {
            try
            {
                // Export logs
                var exportPath = await LoggingService.Instance.ExportLogs();
                
                if (!string.IsNullOrEmpty(exportPath))
                {
                    StatusText = $"Logs exported to {exportPath}";
                    Log.Information($"Logs exported to {exportPath}");
                }
                else
                {
                    StatusText = "Failed to export logs";
                    Log.Error("Failed to export logs");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error exporting logs");
                StatusText = "Error exporting logs";
            }
        }
        
        private async Task RefreshLogsAsync()
        {
            try
            {
                var logContent = await LoggingService.Instance.GetLogContent();
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogContent = logContent;
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error refreshing logs");
            }
        }
        
        private async Task CheckForUpdatesAsync(bool showNotification = true)
        {
            try
            {
                if (showNotification)
                {
                    // Show checking status
                    StatusText = "Checking for updates...";
                    IsIndeterminate = true;
                }
                
                // Check for updates
                var updateInfo = await AutoUpdateService.Instance.CheckForUpdates(true);
                
                if (updateInfo != null)
                {
                    // Show update notification
                    StatusText = $"Update available: {updateInfo.NewVersion}";
                    Progress = 100;
                    IsIndeterminate = false;
                    Log.Information($"Update available: {updateInfo.NewVersion}");
                    
                    // TODO: Show update dialog
                }
                else if (showNotification)
                {
                    StatusText = "No updates available";
                    Progress = 100;
                    IsIndeterminate = false;
                    Log.Information("No updates available");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking for updates");
                
                if (showNotification)
                {
                    StatusText = "Error checking for updates";
                    Progress = 0;
                    IsIndeterminate = false;
                }
            }
        }
        
        private void VisitGitHub()
        {
            try
            {
                // Open GitHub repository
                var url = "https://github.com/maxregnerisch/Deployer";
                
                // Open URL in default browser
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                
                System.Diagnostics.Process.Start(psi);
                
                Log.Information($"Opening URL: {url}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error opening GitHub URL");
            }
        }
        
        private void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
            Log.Information($"Theme toggled to {(IsDarkMode ? "dark" : "light")} mode");
        }
        
        #endregion
    }
}

