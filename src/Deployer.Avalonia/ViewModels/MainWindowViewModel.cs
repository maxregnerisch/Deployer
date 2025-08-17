using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Deployer.Avalonia.UI;

namespace Deployer.Avalonia.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _statusMessage = "Ready";
        private bool _isDeploying = false;
        private bool _isDarkMode = false;
        private double _progress = 0;
        
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool IsDeploying
        {
            get => _isDeploying;
            set
            {
                if (_isDeploying != value)
                {
                    _isDeploying = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanDeploy));
                }
            }
        }
        
        public bool CanDeploy => !IsDeploying;
        
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged();
                    
                    // Update theme
                    ThemeManager.Instance.SetTheme(_isDarkMode ? ThemeType.Dark : ThemeType.Light);
                }
            }
        }
        
        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

