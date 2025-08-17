using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Zafiro.Core;

namespace Deployer.Avalonia.UI
{
    public class ProgressIndicator : IOperationProgress, IDisposable
    {
        private readonly ProgressBar _progressBar;
        private readonly TextBlock _statusTextBlock;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isIndeterminate;
        
        public ProgressIndicator(ProgressBar progressBar, TextBlock statusTextBlock)
        {
            _progressBar = progressBar ?? throw new ArgumentNullException(nameof(progressBar));
            _statusTextBlock = statusTextBlock ?? throw new ArgumentNullException(nameof(statusTextBlock));
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Initialize progress bar
            _progressBar.Minimum = 0;
            _progressBar.Maximum = 100;
            _progressBar.Value = 0;
            _progressBar.IsIndeterminate = true;
            _isIndeterminate = true;
            
            // Initialize status text
            _statusTextBlock.Text = "Ready";
        }
        
        public void Report(double percentage, string message = null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_isIndeterminate && percentage >= 0)
                {
                    _progressBar.IsIndeterminate = false;
                    _isIndeterminate = false;
                }
                
                if (percentage >= 0)
                {
                    _progressBar.Value = percentage;
                }
                
                if (!string.IsNullOrEmpty(message))
                {
                    _statusTextBlock.Text = message;
                }
            });
        }
        
        public void SetIndeterminate(string message = null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (!_isIndeterminate)
                {
                    _progressBar.IsIndeterminate = true;
                    _isIndeterminate = true;
                }
                
                if (!string.IsNullOrEmpty(message))
                {
                    _statusTextBlock.Text = message;
                }
            });
        }
        
        public void SetCompleted(string message = "Completed")
        {
            Dispatcher.UIThread.Post(() =>
            {
                _progressBar.IsIndeterminate = false;
                _isIndeterminate = false;
                _progressBar.Value = 100;
                _statusTextBlock.Text = message;
            });
        }
        
        public void SetFailed(string message = "Failed")
        {
            Dispatcher.UIThread.Post(() =>
            {
                _progressBar.IsIndeterminate = false;
                _isIndeterminate = false;
                _statusTextBlock.Text = message;
            });
        }
        
        public void Reset()
        {
            Dispatcher.UIThread.Post(() =>
            {
                _progressBar.Value = 0;
                _progressBar.IsIndeterminate = true;
                _isIndeterminate = true;
                _statusTextBlock.Text = "Ready";
            });
        }
        
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;
        
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
        
        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }
    }
}

