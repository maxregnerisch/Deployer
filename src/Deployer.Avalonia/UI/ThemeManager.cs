using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using Serilog;

namespace Deployer.Avalonia.UI
{
    public enum ThemeType
    {
        Light,
        Dark,
        System
    }

    public class ThemeManager
    {
        private static ThemeManager _instance;
        private ThemeType _currentTheme = ThemeType.Light;
        
        public static ThemeManager Instance => _instance ??= new ThemeManager();
        
        private ThemeManager()
        {
            // Private constructor for singleton pattern
        }
        
        public ThemeType CurrentTheme => _currentTheme;
        
        public event EventHandler<ThemeType> ThemeChanged;
        
        public void SetTheme(ThemeType theme)
        {
            if (_currentTheme == theme)
            {
                return;
            }
            
            _currentTheme = theme;
            
            try
            {
                ApplyTheme(theme);
                SaveThemePreference(theme);
                ThemeChanged?.Invoke(this, theme);
                
                Log.Information($"Theme changed to {theme}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error changing theme");
            }
        }
        
        public void Initialize()
        {
            try
            {
                var savedTheme = LoadThemePreference();
                _currentTheme = savedTheme;
                
                if (savedTheme == ThemeType.System)
                {
                    // Detect system theme
                    var systemTheme = DetectSystemTheme();
                    ApplyTheme(systemTheme);
                }
                else
                {
                    ApplyTheme(savedTheme);
                }
                
                Log.Information($"Theme initialized to {_currentTheme}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing theme");
                
                // Default to light theme on error
                _currentTheme = ThemeType.Light;
                ApplyTheme(ThemeType.Light);
            }
        }
        
        private void ApplyTheme(ThemeType theme)
        {
            var actualTheme = theme == ThemeType.System ? DetectSystemTheme() : theme;
            
            // Apply theme to Avalonia application
            Application.Current.RequestedThemeVariant = actualTheme == ThemeType.Dark 
                ? ThemeVariant.Dark 
                : ThemeVariant.Light;
        }
        
        private ThemeType DetectSystemTheme()
        {
            try
            {
                // Try to detect system theme using Windows registry
                // This is a simplified implementation
                return DateTime.Now.Hour >= 19 || DateTime.Now.Hour < 7 
                    ? ThemeType.Dark 
                    : ThemeType.Light;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error detecting system theme");
                return ThemeType.Light;
            }
        }
        
        private void SaveThemePreference(ThemeType theme)
        {
            try
            {
                var appDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Deployer");
                
                Directory.CreateDirectory(appDataFolder);
                
                var preferencesFile = Path.Combine(appDataFolder, "theme_preference.txt");
                File.WriteAllText(preferencesFile, theme.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving theme preference");
            }
        }
        
        private ThemeType LoadThemePreference()
        {
            try
            {
                var appDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Deployer");
                
                var preferencesFile = Path.Combine(appDataFolder, "theme_preference.txt");
                
                if (File.Exists(preferencesFile))
                {
                    var themeString = File.ReadAllText(preferencesFile).Trim();
                    
                    if (Enum.TryParse<ThemeType>(themeString, out var theme))
                    {
                        return theme;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading theme preference");
            }
            
            return ThemeType.Light; // Default to light theme
        }
    }
}

