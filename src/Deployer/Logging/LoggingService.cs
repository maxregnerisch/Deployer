using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Deployer.Logging
{
    public class LoggingService
    {
        private static LoggingService _instance;
        private readonly LoggingLevelSwitch _levelSwitch;
        private string _logFilePath;
        
        public static LoggingService Instance => _instance ??= new LoggingService();
        
        private LoggingService()
        {
            _levelSwitch = new LoggingLevelSwitch(LogEventLevel.Information);
            InitializeLogger();
        }
        
        public LogEventLevel CurrentLevel
        {
            get => _levelSwitch.MinimumLevel;
            set => _levelSwitch.MinimumLevel = value;
        }
        
        public string LogFilePath => _logFilePath;
        
        private void InitializeLogger()
        {
            try
            {
                var appDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Deployer",
                    "Logs");
                
                Directory.CreateDirectory(appDataFolder);
                
                _logFilePath = Path.Combine(appDataFolder, $"Deployer_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(_levelSwitch)
                    .WriteTo.File(_logFilePath, rollingInterval: RollingInterval.Day)
                    .WriteTo.Debug()
                    .CreateLogger();
                
                Log.Information("Logging initialized");
            }
            catch (Exception ex)
            {
                // Fallback to console logging if file logging fails
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(_levelSwitch)
                    .WriteTo.Debug()
                    .CreateLogger();
                
                Log.Error(ex, "Error initializing file logger");
            }
        }
        
        public void SetVerbosity(LogEventLevel level)
        {
            _levelSwitch.MinimumLevel = level;
            Log.Information($"Log level set to {level}");
        }
        
        public async Task<string> ExportLogs(string destinationPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(destinationPath))
                {
                    // Use default location if none provided
                    var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    destinationPath = Path.Combine(desktopPath, $"Deployer_Logs_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                }
                
                // Ensure the log file exists
                if (!File.Exists(_logFilePath))
                {
                    Log.Error("Log file does not exist");
                    return null;
                }
                
                // Copy the log file to the destination
                File.Copy(_logFilePath, destinationPath, true);
                
                Log.Information($"Logs exported to {destinationPath}");
                return destinationPath;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error exporting logs");
                return null;
            }
        }
        
        public async Task<string> GetLogContent()
        {
            try
            {
                if (!File.Exists(_logFilePath))
                {
                    return "No log file found";
                }
                
                return await File.ReadAllTextAsync(_logFilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error reading log content");
                return $"Error reading log content: {ex.Message}";
            }
        }
        
        public void ClearLogs()
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    File.WriteAllText(_logFilePath, string.Empty);
                    Log.Information("Logs cleared");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error clearing logs");
            }
        }
    }
}

