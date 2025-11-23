using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyShop.Client.ViewModels.Settings;

/// <summary>
/// ViewModel for Settings page - manage app preferences
/// Persists settings via ISettingsStorage
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsStorage _settingsStorage;
    private readonly IToastService _toastHelper;

    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // Settings properties
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _theme = "System";

    public ObservableCollection<ThemeOption> ThemeOptions { get; } = new()
    {
        new ThemeOption { Name = "System Default", Value = "System", Icon = "\uE771" },
        new ThemeOption { Name = "Light", Value = "Light", Icon = "\uE706" },
        new ThemeOption { Name = "Dark", Value = "Dark", Icon = "\uE708" }
    };

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _language = "vi-VN";

    public ObservableCollection<LanguageOption> LanguageOptions { get; } = new()
    {
        new LanguageOption { Code = "vi-VN", Name = "Tiếng Việt", Flag = "🇻🇳" },
        new LanguageOption { Code = "en-US", Name = "English", Flag = "🇺🇸" }
    };

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _notificationsEnabled = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _restoreLastPage = true;

    // Page size preferences
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private int _productsPageSize = 20;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private int _ordersPageSize = 15;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private int _customersPageSize = 20;

    public ObservableCollection<int> PageSizeOptions { get; } = new() { 10, 15, 20, 25, 50, 100 };

    // Extended notification settings
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _enableSoundNotifications = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _notifyOnLowStock = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _notifyOnNewOrders = true;

    // Cache management
    [ObservableProperty] private string _cacheSize = "0 MB";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _enableOfflineMode = false;

    // Shop information settings
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _shopName = "MyShop 2025";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _address = string.Empty;

    // Timezone settings
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private int _selectedTimezoneIndex = 0;

    public ObservableCollection<string> TimezoneOptions { get; } = new()
    {
        "UTC+7 (Vietnam)",
        "UTC+8 (Singapore)",
        "UTC+9 (Japan)",
        "UTC+0 (GMT)",
        "UTC-5 (EST)",
        "UTC-8 (PST)"
    };

    // Track if settings changed
    private AppSettings? _originalSettings;

    public SettingsViewModel(
        ISettingsStorage settingsStorage,
        IToastService toastHelper)
    {
        _settingsStorage = settingsStorage;
        _toastHelper = toastHelper;
    }

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    [RelayCommand]
    private async Task CalculateCacheSizeAsync()
    {
        try
        {
            var cacheFolder = ApplicationData.Current.LocalCacheFolder;
            var properties = await cacheFolder.GetBasicPropertiesAsync();
            double sizeInMB = properties.Size / 1024.0 / 1024.0;
            CacheSize = $"{sizeInMB:F2} MB";
        }
        catch
        {
            CacheSize = "0 MB";
        }
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        try
        {
            var cacheFolder = ApplicationData.Current.LocalCacheFolder;
            var files = await cacheFolder.GetFilesAsync();
            foreach (var file in files)
            {
                await file.DeleteAsync();
            }
            await CalculateCacheSizeAsync();
        }
        catch
        {
            // Silently fail
        }
    }

    [RelayCommand]
    private async Task ExportSettingsAsync()
    {
        try
        {
            var savePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("JSON Settings", new List<string> { ".json" });
            savePicker.SuggestedFileName = "myshop-settings";

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                var settings = await _settingsStorage.GetAsync();
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                await FileIO.WriteTextAsync(file, json);
            }
        }
        catch
        {
            // Silently fail
        }
    }

    [RelayCommand]
    private async Task ImportSettingsAsync()
    {
        try
        {
            var openPicker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".json");

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var json = await FileIO.ReadTextAsync(file);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    await _settingsStorage.SaveAsync(settings);
                    await LoadAsync();
                }
            }
        }
        catch
        {
            // Silently fail
        }
    }

    /// <summary>
    /// Load settings from storage
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var settings = await _settingsStorage.GetAsync();
            
            Theme = settings.Theme;
            Language = settings.Language;
            NotificationsEnabled = settings.NotificationsEnabled;
            RestoreLastPage = settings.RestoreLastPage;
            ProductsPageSize = settings.ProductsPageSize;
            OrdersPageSize = settings.OrdersPageSize;
            CustomersPageSize = settings.CustomersPageSize;
            EnableSoundNotifications = settings.EnableSoundNotifications;
            NotifyOnLowStock = settings.NotifyOnLowStock;
            NotifyOnNewOrders = settings.NotifyOnNewOrders;
            EnableOfflineMode = settings.EnableOfflineMode;
            ShopName = settings.ShopName ?? "MyShop 2025";
            Address = settings.Address ?? string.Empty;
            SelectedTimezoneIndex = settings.SelectedTimezoneIndex;

            // Store original for change detection
            _originalSettings = settings;

            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Loaded: Theme={Theme}, Language={Language}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Load error: {ex.Message}");
            ErrorMessage = "Failed to load settings.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Save settings to storage
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var settings = new AppSettings
            {
                Theme = Theme,
                Language = Language,
                NotificationsEnabled = NotificationsEnabled,
                RestoreLastPage = RestoreLastPage,
                ProductsPageSize = ProductsPageSize,
                OrdersPageSize = OrdersPageSize,
                CustomersPageSize = CustomersPageSize,
                EnableSoundNotifications = EnableSoundNotifications,
                NotifyOnLowStock = NotifyOnLowStock,
                NotifyOnNewOrders = NotifyOnNewOrders,
                EnableOfflineMode = EnableOfflineMode,
                ShopName = ShopName,
                Address = Address,
                SelectedTimezoneIndex = SelectedTimezoneIndex
            };

            await _settingsStorage.SaveAsync(settings);

            // Update original settings
            _originalSettings = settings;

            _toastHelper.ShowSuccess("Settings saved successfully!");
            
            // Apply theme and language immediately
            ApplyThemeAndLanguage();

            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Saved: Theme={Theme}, Language={Language}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Save error: {ex.Message}");
            ErrorMessage = "Failed to save settings.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanSave() => !IsLoading && HasChanges();

    /// <summary>
    /// Reset settings to defaults
    /// </summary>
    [RelayCommand]
    private async Task ResetAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            await _settingsStorage.ResetAsync();
            
            // Reload defaults
            await LoadAsync();

            _toastHelper.ShowInfo("Settings reset to defaults.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Reset error: {ex.Message}");
            ErrorMessage = "Failed to reset settings.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Check if settings have changed from original
    /// </summary>
    private bool HasChanges()
    {
        if (_originalSettings == null) return true;

        return Theme != _originalSettings.Theme ||
               Language != _originalSettings.Language ||
               NotificationsEnabled != _originalSettings.NotificationsEnabled ||
               RestoreLastPage != _originalSettings.RestoreLastPage ||
               ProductsPageSize != _originalSettings.ProductsPageSize ||
               OrdersPageSize != _originalSettings.OrdersPageSize ||
               CustomersPageSize != _originalSettings.CustomersPageSize ||
               EnableSoundNotifications != _originalSettings.EnableSoundNotifications ||
               NotifyOnLowStock != _originalSettings.NotifyOnLowStock ||
               NotifyOnNewOrders != _originalSettings.NotifyOnNewOrders ||
               EnableOfflineMode != _originalSettings.EnableOfflineMode ||
               ShopName != _originalSettings.ShopName ||
               Address != _originalSettings.Address ||
               SelectedTimezoneIndex != _originalSettings.SelectedTimezoneIndex;
    }

    /// <summary>
    /// Apply theme and language settings immediately
    /// </summary>
    private void ApplyThemeAndLanguage()
    {
        try
        {
            // Apply theme
            if (App.MainWindow?.Content is Microsoft.UI.Xaml.FrameworkElement root)
            {
                var requestedTheme = Theme.ToLower() switch
                {
                    "light" => Microsoft.UI.Xaml.ElementTheme.Light,
                    "dark" => Microsoft.UI.Xaml.ElementTheme.Dark,
                    _ => Microsoft.UI.Xaml.ElementTheme.Default
                };

                root.RequestedTheme = requestedTheme;
                System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Applied theme: {requestedTheme}");
            }

            // Apply language (requires app restart for full effect)
            // For immediate effect on current page, you would need to:
            // 1. Update resource dictionaries
            // 2. Re-bind all text elements
            // For simplicity, show a notification that language will apply on restart
            if (Language != _originalSettings?.Language)
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Language changed to: {Language}");
                _toastHelper.ShowInfo("Language changes will take effect after app restart.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] ApplyThemeAndLanguage error: {ex.Message}");
        }
    }
}
