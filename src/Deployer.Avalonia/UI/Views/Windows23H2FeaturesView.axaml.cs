using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Deployer;

namespace Deployer.Avalonia.UI.Views
{
    public partial class Windows23H2FeaturesView : UserControl
    {
        private readonly ProgressIndicator _progressIndicator;
        
        public Windows23H2FeaturesView()
        {
            InitializeComponent();
            
            // Get progress indicator from main window
            var mainWindow = (MainWindow)App.Current.MainWindow;
            var progressBar = mainWindow.FindControl<ProgressBar>("ProgressBar");
            var statusTextBlock = mainWindow.FindControl<TextBlock>("StatusTextBlock");
            _progressIndicator = new ProgressIndicator(progressBar, statusTextBlock);
            
            // Load data
            LoadData();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void LoadData()
        {
            try
            {
                // Load features
                var featuresItemsControl = this.FindControl<ItemsControl>("FeaturesItemsControl");
                if (featuresItemsControl != null)
                {
                    featuresItemsControl.Items = Windows23H2Features.GetNewFeatures();
                }
                
                // Load requirements
                var requirementsItemsControl = this.FindControl<ItemsControl>("RequirementsItemsControl");
                if (requirementsItemsControl != null)
                {
                    requirementsItemsControl.Items = Windows23H2Features.GetSystemRequirements().ToList();
                }
                
                // Load optimizations
                var optimizationsItemsControl = this.FindControl<ItemsControl>("OptimizationsItemsControl");
                if (optimizationsItemsControl != null)
                {
                    optimizationsItemsControl.Items = Windows23H2Features.GetOptimizations();
                }
            }
            catch (Exception ex)
            {
                // Error handling
                var compatibilityResultTextBlock = this.FindControl<TextBlock>("CompatibilityResultTextBlock");
                if (compatibilityResultTextBlock != null)
                {
                    compatibilityResultTextBlock.Text = $"Error loading data: {ex.Message}";
                    compatibilityResultTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }
        
        private async void OnCheckCompatibilityClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show progress
                _progressIndicator.SetIndeterminate("Checking system compatibility...");
                
                // Check compatibility
                var isCompatible = await Windows23H2Features.CheckSystemCompatibility();
                
                // Update UI
                var compatibilityResultTextBlock = this.FindControl<TextBlock>("CompatibilityResultTextBlock");
                if (compatibilityResultTextBlock != null)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        compatibilityResultTextBlock.Text = isCompatible
                            ? "Your system is compatible with Windows 11 23H2! You can proceed with the deployment."
                            : "Your system does not meet all the requirements for Windows 11 23H2. Some features may not work correctly.";
                        
                        compatibilityResultTextBlock.Foreground = new SolidColorBrush(
                            isCompatible ? Colors.Green : Colors.Orange);
                    });
                }
                
                // Complete progress
                _progressIndicator.SetCompleted("Compatibility check completed");
            }
            catch (Exception ex)
            {
                // Error handling
                _progressIndicator.SetFailed("Compatibility check failed");
                
                var compatibilityResultTextBlock = this.FindControl<TextBlock>("CompatibilityResultTextBlock");
                if (compatibilityResultTextBlock != null)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        compatibilityResultTextBlock.Text = $"Error checking compatibility: {ex.Message}";
                        compatibilityResultTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    });
                }
            }
        }
        
        private async void OnApplyOptimizationClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get optimization name
                var button = (Button)sender;
                var optimizationName = button.Tag.ToString();
                
                // Show progress
                _progressIndicator.SetIndeterminate($"Applying optimization: {optimizationName}...");
                
                // Apply optimization
                var success = await Windows23H2Features.ApplyOptimization(optimizationName);
                
                // Update UI
                if (success)
                {
                    _progressIndicator.SetCompleted($"Successfully applied: {optimizationName}");
                    
                    // Disable button
                    button.IsEnabled = false;
                    button.Content = "Applied";
                }
                else
                {
                    _progressIndicator.SetFailed($"Failed to apply: {optimizationName}");
                }
            }
            catch (Exception ex)
            {
                // Error handling
                _progressIndicator.SetFailed("Failed to apply optimization");
                
                var compatibilityResultTextBlock = this.FindControl<TextBlock>("CompatibilityResultTextBlock");
                if (compatibilityResultTextBlock != null)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        compatibilityResultTextBlock.Text = $"Error applying optimization: {ex.Message}";
                        compatibilityResultTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    });
                }
            }
        }
    }
}

