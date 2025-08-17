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
        private readonly string _logFilePath;
        private readonly Logger _logger;
        
        public static LoggingService Instance => _instance ??= new LoggingService();
        
        private LoggingService()
        {
            // Create logs directory in AppData
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Deployer");
            
            Directory.CreateDirectory(appDataFolder);
            
            // Set log file path
            _logFilePath = Path.Combine(appDataFolder, "deployer.log");
            
            // Configure Serilog
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(_logFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
            
            // Set global logger
            Log.Logger = _logger;
            
            Log.Information("LoggingService initialized");
        }
        
        public async Task<string> GetLogContent()
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    return await File.ReadAllTextAsync(_logFilePath);
                }
                
                return "No logs available.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error reading log file");
                return $"Error reading log file: {ex.Message}";
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
        
        public async Task<string> ExportLogs()
        {
            try
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var exportPath = Path.Combine(desktopPath, $"Deployer_Logs_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                
                if (File.Exists(_logFilePath))
                {
                    var logContent = await File.ReadAllTextAsync(_logFilePath);
                    await File.WriteAllTextAsync(exportPath, logContent);
                    
                    Log.Information($"Logs exported to {exportPath}");
                    return exportPath;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error exporting logs");
                return null;
            }
        }
    }
}

