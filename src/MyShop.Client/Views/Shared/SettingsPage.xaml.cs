using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Settings;
using MyShop.Client.Services;
using System;

namespace MyShop.Client.Views.Shared;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }
    private bool _isInitializing = true;
    private string _originalTheme = "Light"; // Track original theme for revert

    public SettingsPage()
    {
        LoggingService.Instance.Debug("→ SettingsPage()");
        try
        {
            this.InitializeComponent();
            LoggingService.Instance.Debug("InitializeComponent completed");
            
            // Get ViewModel from DI
            ViewModel = App.Current.Services.GetRequiredService<SettingsViewModel>();
            this.DataContext = ViewModel;
            LoggingService.Instance.Debug("ViewModel retrieved from DI and DataContext set");
            
            // Initialize theme toggle based on current theme
            ThemeToggle.IsOn = ThemeManager.CurrentTheme == ThemeManager.ThemeType.Dark;
            
            SetupKeyboardShortcuts();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("SettingsPage constructor failed", ex);
            throw;
        }
        finally
        {
            LoggingService.Instance.Debug("← SettingsPage()");
        }
    }

    private void SetupKeyboardShortcuts()
    {
        // Ctrl+S: Save settings
        var saveShortcut = new Microsoft.UI.Xaml.Input.KeyboardAccelerator 
        { 
            Key = Windows.System.VirtualKey.S, 
            Modifiers = Windows.System.VirtualKeyModifiers.Control 
        };
        saveShortcut.Invoked += async (s, e) => { await ViewModel.SaveCommand.ExecuteAsync(null); e.Handled = true; };
        KeyboardAccelerators.Add(saveShortcut);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        LoggingService.Instance.Debug("→ OnNavigatedTo");
        try
        {
            base.OnNavigatedTo(e);
            
            // Load settings and wait for completion
            await ViewModel.LoadCommand.ExecuteAsync(null);
            LoggingService.Instance.Debug("Settings loaded");
            
            // Wait for UI to stabilize before allowing toggle events
            // This prevents the Toggled event from firing due to binding updates
            await System.Threading.Tasks.Task.Delay(100);
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("OnNavigatedTo failed", ex);
        }
        finally
        {
            // Sync toggle with saved theme after loading
            ThemeToggle.IsOn = ViewModel.Theme == "Dark";
            
            // Store original theme for revert on navigation away without save
            _originalTheme = ViewModel.Theme;
            
            // Allow toggle events only after load completes and UI stabilizes
            _isInitializing = false;
            
            // Subscribe to PropertyChanged to update _originalTheme when saved
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            LoggingService.Instance.Debug("← OnNavigatedTo");
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Update _originalTheme when theme is saved (after SaveCommand completes)
        if (e.PropertyName == nameof(ViewModel.Theme) && !ViewModel.IsLoading)
        {
            _originalTheme = ViewModel.Theme;
            LoggingService.Instance.Debug($"Updated original theme to: {_originalTheme}");
        }
    }

    private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
    {
        // Skip during page initialization
        if (_isInitializing) return;

        if (sender is ToggleSwitch toggle)
        {
            try
            {
                // Update ViewModel.Theme property for tracking
                ViewModel.Theme = toggle.IsOn ? "Dark" : "Light";
                
                // Apply theme preview immediately
                var newTheme = toggle.IsOn ? ThemeManager.ThemeType.Dark : ThemeManager.ThemeType.Light;
                ThemeManager.ApplyTheme(newTheme);
                
                LoggingService.Instance.Information($"Theme preview applied: {ViewModel.Theme} (save to persist)");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to apply theme preview", ex);
                // Revert toggle on error
                toggle.Toggled -= ThemeToggle_Toggled;
                toggle.IsOn = !toggle.IsOn;
                toggle.Toggled += ThemeToggle_Toggled;
            }
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        
        // Unsubscribe from PropertyChanged to avoid memory leaks
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        
        // If theme changed but wasn't saved, revert to original
        if (ViewModel.Theme != _originalTheme)
        {
            var savedTheme = _originalTheme == "Dark" ? ThemeManager.ThemeType.Dark : ThemeManager.ThemeType.Light;
            ThemeManager.ApplyTheme(savedTheme);
            LoggingService.Instance.Information($"Reverted theme to original: {_originalTheme} (changes not saved)");
        }
    }
}
