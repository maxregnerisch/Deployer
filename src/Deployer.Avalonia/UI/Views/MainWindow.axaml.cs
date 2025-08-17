using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Deployer.Logging;
using Serilog;

namespace Deployer.Avalonia.UI.Views
{
    public partial class MainWindow : Window
    {
        private readonly ProgressIndicator _progressIndicator;
        private readonly ThemeManager _themeManager;
        private bool _isDarkMode = false;
        
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
            // Initialize progress indicator
            var progressBar = this.FindControl<ProgressBar>("ProgressBar");
            var statusTextBlock = this.FindControl<TextBlock>("StatusTextBlock");
            _progressIndicator = new ProgressIndicator(progressBar, statusTextBlock);
            
            // Initialize theme manager
            _themeManager = ThemeManager.Instance;
            _themeManager.ThemeChanged += OnThemeChanged;
            _themeManager.Initialize();
            
            // Initialize UI
            InitializeUI();
            
            // Removed auto-update functionality
            
            // Check device compatibility
            CheckDeviceCompatibilityAsync();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void InitializeUI()
        {
            try
            {
                // Set version text
                var versionTextBlock = this.FindControl<TextBlock>("VersionTextBlock");
                versionTextBlock.Text = $"Version 23H2";
                
                // Initialize logs
                RefreshLogs();
                
                // Update theme button
                UpdateThemeButton();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing UI");
            }
        }
        
        private void OnThemeToggleClicked(object sender, RoutedEventArgs e)
        {
            _isDarkMode = !_isDarkMode;
            _themeManager.SetTheme(_isDarkMode ? ThemeType.Dark : ThemeType.Light);
            UpdateThemeButton();
        }
        
        private void UpdateThemeButton()
        {
            var themeButton = this.FindControl<Button>("ThemeToggleButton");
            themeButton.Content = _isDarkMode ? "☀️" : "🌙";
        }
        
        private void OnThemeChanged(object sender, ThemeType themeType)
        {
            _isDarkMode = themeType == ThemeType.Dark;
            UpdateThemeButton();
        }
        
        private void OnSettingsClicked(object sender, RoutedEventArgs e)
        {
            // TODO: Open settings window
            Log.Information("Settings button clicked");
        }
        
        private void OnBrowseImageClicked(object sender, RoutedEventArgs e)
        {
            // TODO: Open file dialog
            Log.Information("Browse image button clicked");
        }
        
        private void OnDeployClicked(object sender, RoutedEventArgs e)
        {
            // TODO: Start deployment
            Log.Information("Deploy button clicked");
            
            // Show progress
            _progressIndicator.SetIndeterminate("Starting deployment...");
            
            // Enable cancel button
            var cancelButton = this.FindControl<Button>("CancelButton");
            cancelButton.IsEnabled = true;
            
            // Disable deploy button
            var deployButton = this.FindControl<Button>("DeployButton");
            deployButton.IsEnabled = false;
            
            // Simulate deployment progress
            SimulateDeploymentProgress();
        }
        
        private async void SimulateDeploymentProgress()
        {
            try
            {
                for (int i = 0; i <= 100; i += 5)
                {
                    await Task.Delay(500);
                    
                    if (_progressIndicator.CancellationToken.IsCancellationRequested)
                    {
                        _progressIndicator.SetFailed("Deployment cancelled");
                        Log.Information("Deployment cancelled");
                        break;
                    }
                    
                    _progressIndicator.Report(i, $"Deploying Windows 11 23H2... {i}%");
                    Log.Information($"Deployment progress: {i}%");
                }
                
                if (!_progressIndicator.CancellationToken.IsCancellationRequested)
                {
                    _progressIndicator.SetCompleted("Deployment completed successfully");
                    Log.Information("Deployment completed successfully");
                }
                
                // Disable cancel button
                var cancelButton = this.FindControl<Button>("CancelButton");
                cancelButton.IsEnabled = false;
                
                // Enable deploy button
                var deployButton = this.FindControl<Button>("DeployButton");
                deployButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during deployment");
                _progressIndicator.SetFailed("Deployment failed");
                
                // Disable cancel button
                var cancelButton = this.FindControl<Button>("CancelButton");
                cancelButton.IsEnabled = false;
                
                // Enable deploy button
                var deployButton = this.FindControl<Button>("DeployButton");
                deployButton.IsEnabled = true;
            }
        }
        
        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            // Cancel deployment
            _progressIndicator.Cancel();
            Log.Information("Cancel button clicked");
        }
        
        private void OnRefreshDeviceInfoClicked(object sender, RoutedEventArgs e)
        {
            // Refresh device info
            Log.Information("Refresh device info button clicked");
            CheckDeviceCompatibilityAsync();
        }
        
        private async void CheckDeviceCompatibilityAsync()
        {
            try
            {
                // Update UI to show checking status
                var compatibilityStatusTextBlock = this.FindControl<TextBlock>("CompatibilityStatusTextBlock");
                var compatibilityDetailsTextBlock = this.FindControl<TextBlock>("CompatibilityDetailsTextBlock");
                
                Dispatcher.UIThread.Post(() =>
                {
                    compatibilityStatusTextBlock.Text = "Checking compatibility...";
                    compatibilityDetailsTextBlock.Text = "Please wait...";
                });
                
                // Check device compatibility
                var compatibilityInfo = await DeviceCompatibilityChecker.CheckDeviceCompatibility();
                
                if (compatibilityInfo == null)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        compatibilityStatusTextBlock.Text = "Compatibility check failed";
                        compatibilityDetailsTextBlock.Text = "Could not determine device compatibility";
                        compatibilityStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    });
                    
                    return;
                }
                
                // Update UI with compatibility info
                Dispatcher.UIThread.Post(() =>
                {
                    compatibilityStatusTextBlock.Text = compatibilityInfo.IsCompatible 
                        ? "Compatible with Windows 11 23H2" 
                        : "Not compatible with Windows 11 23H2";
                    
                    compatibilityDetailsTextBlock.Text = compatibilityInfo.CompatibilityMessage;
                    
                    compatibilityStatusTextBlock.Foreground = new SolidColorBrush(
                        compatibilityInfo.IsCompatible ? Colors.Green : Colors.Red);
                    
                    // Update device info
                    var deviceNameTextBlock = this.FindControl<TextBlock>("DeviceNameTextBlock");
                    deviceNameTextBlock.Text = compatibilityInfo.DeviceName;
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking device compatibility");
                
                Dispatcher.UIThread.Post(() =>
                {
                    var compatibilityStatusTextBlock = this.FindControl<TextBlock>("CompatibilityStatusTextBlock");
                    var compatibilityDetailsTextBlock = this.FindControl<TextBlock>("CompatibilityDetailsTextBlock");
                    
                    compatibilityStatusTextBlock.Text = "Compatibility check failed";
                    compatibilityDetailsTextBlock.Text = $"Error: {ex.Message}";
                    compatibilityStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                });
            }
        }
        
        private void OnClearLogsClicked(object sender, RoutedEventArgs e)
        {
            // Clear logs
            LoggingService.Instance.ClearLogs();
            RefreshLogs();
            Log.Information("Logs cleared");
        }
        
        private async void OnExportLogsClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Export logs
                var exportPath = await LoggingService.Instance.ExportLogs();
                
                if (!string.IsNullOrEmpty(exportPath))
                {
                    Log.Information($"Logs exported to {exportPath}");
                    _progressIndicator.Report(100, $"Logs exported to {exportPath}");
                }
                else
                {
                    Log.Error("Failed to export logs");
                    _progressIndicator.Report(0, "Failed to export logs");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error exporting logs");
                _progressIndicator.Report(0, "Error exporting logs");
            }
        }
        
        private async void RefreshLogs()
        {
            try
            {
                var logsTextBox = this.FindControl<TextBox>("LogsTextBox");
                var logContent = await LoggingService.Instance.GetLogContent();
                
                Dispatcher.UIThread.Post(() =>
                {
                    logsTextBox.Text = logContent;
                    logsTextBox.CaretIndex = logContent.Length;
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error refreshing logs");
            }
        }
        
        // Auto-update functionality has been removed
        private void CheckForUpdatesAsync()
        {
            // This method is intentionally empty as auto-update functionality has been removed
        }
        
        private void OnCheckForUpdatesClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Auto-update functionality has been removed
                _progressIndicator.Report(100, "Auto-update functionality has been removed. Please check GitHub for updates.");
                Log.Information("Auto-update functionality has been removed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling update check");
                _progressIndicator.Report(0, "Error handling update check");
            }
        }
        
        private void OnVisitGitHubClicked(object sender, RoutedEventArgs e)
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
    }
}
