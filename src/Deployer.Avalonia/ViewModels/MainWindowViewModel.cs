using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;

namespace Deployer.Avalonia.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private string _statusText;
        private double _progress;
        private bool _isIndeterminate;
        
        public MainWindowViewModel()
        {
            StatusText = "Ready";
            Progress = 0;
            IsIndeterminate = false;
        }
        
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
    }
}

