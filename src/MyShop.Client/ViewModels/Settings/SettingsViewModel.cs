using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Services.Configuration;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Plugins.Repositories.Mocks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

// Use alias to avoid ambiguity with MockSettingsRepository.AppSettings
using AppSettings = MyShop.Shared.Models.AppSettings;
using PaginationSettings = MyShop.Shared.Models.PaginationSettings;

namespace MyShop.Client.ViewModels.Settings;

/// <summary>
/// ViewModel for Settings page - manage app preferences
/// Persists settings via ISettingsStorage
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IConfigurationService _configService;
    private readonly ISettingsStorage _settingsStorage;
    private readonly IToastService _toastHelper;
    private readonly IPaginationService _paginationService;
    private readonly MockSettingsRepository _mockSettingsRepository;
    private readonly ISystemActivationRepository _activationRepository;
    private readonly IAuthRepository _authRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // Trial properties
    [ObservableProperty] private int _trialDaysRemaining = 0;
    [ObservableProperty] private string _upgradeProUrl = "https://facebook.com";
    [ObservableProperty] private string _supportUrl = "https://facebook.com/myshop.support";
    [ObservableProperty] private string _trialCode = string.Empty;
    
    // License/Trial visibility properties
    [ObservableProperty] private bool _isAdmin = false;
    [ObservableProperty] private bool _isTrialAdmin = false;
    [ObservableProperty] private bool _isPermanentAdmin = false;
    [ObservableProperty] private bool _showTrialTab = false;
    [ObservableProperty] private string _licenseType = "None";
    [ObservableProperty] private DateTime? _licenseExpiresAt = null;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // About info - read from assembly
    public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    public string ReleaseYear => $"© {DateTime.Now.Year} MyShop. All rights reserved.";

    // Developer tab properties
    public bool UseMockData
    {
        get => _configService.FeatureFlags.UseMockData;
        set
        {
            if (_configService.FeatureFlags.UseMockData != value)
            {
                // Note: This only updates the in-memory value
                // Actual config change requires editing appsettings.Development.json
                OnPropertyChanged();
                OnPropertyChanged(nameof(DataModeDisplay));
            }
        }
    }

    public string ServerUrl
    {
        get => _configService.Api.BaseUrl;
        set => OnPropertyChanged();
    }

    public string DataModeDisplay => UseMockData ? "Mock Data" : "Real API";
    
    [ObservableProperty]
    private string _currentUserEmail = "Not logged in";
    
    [ObservableProperty]
    private string _currentUserRole = "Unknown";
    
#if DEBUG
    public string BuildConfiguration => "Debug";
#else
    public string BuildConfiguration => "Release";
#endif

    /// <summary>
    /// Gets formatted debug information for clipboard copy
    /// </summary>
    public string GetDebugInfo()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== MyShop 2025 Debug Info ===");
        sb.AppendLine($"Version: {AppVersion}");
        sb.AppendLine($"Build: {BuildConfiguration}");
        sb.AppendLine($"Data Mode: {DataModeDisplay}");
        sb.AppendLine($"Server URL: {ServerUrl}");
        sb.AppendLine($"User: {CurrentUserEmail}");
        sb.AppendLine($"Role: {CurrentUserRole}");
        sb.AppendLine($"Theme: {Theme}");
        sb.AppendLine($"Language: {Language}");
        sb.AppendLine($"OS: {Environment.OSVersion}");
        sb.AppendLine($".NET: {Environment.Version}");
        sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("==============================");
        return sb.ToString();
    }

    // Settings properties
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _theme = "Light"; // Changed default from System to Light

    // TODO: Uncomment when Appearance tab UI is implemented
    // Theme options collection causes WinUI binding issues with emoji characters when not bound to UI
    // public ObservableCollection<ThemeOption> ThemeOptions { get; } = new()
    // {
    //     new ThemeOption { Name = "System Default", Value = "System", Icon = "\uE771" },
    //     new ThemeOption { Name = "Light", Value = "Light", Icon = "\uE706" },
    //     new ThemeOption { Name = "Dark", Value = "Dark", Icon = "\uE708" }
    // };

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _language = "en-US"; // Changed default from vi-VN to en-US

    // TODO: Uncomment when Appearance tab UI is implemented
    // Language options collection causes WinUI binding issues with emoji characters when not bound to UI
    // public ObservableCollection<LanguageOption> LanguageOptions { get; } = new()
    // {
    //     new LanguageOption { Code = "vi-VN", Name = "Tiếng Việt", Flag = "🇻🇳" },
    //     new LanguageOption { Code = "en-US", Name = "English", Flag = "🇺🇸" }
    // };

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _notificationsEnabled = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _restoreLastPage = true;

    // Page size preferences
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private int _productsPageSize = Core.Common.PaginationConstants.ProductsPageSize;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private int _ordersPageSize = Core.Common.PaginationConstants.OrdersPageSize;

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
        IConfigurationService configService,
        ISettingsStorage settingsStorage,
        IToastService toastHelper,
        IPaginationService paginationService,
        ISystemActivationRepository activationRepository,
        IAuthRepository authRepository,
        INavigationService navigationService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _settingsStorage = settingsStorage;
        _toastHelper = toastHelper;
        _paginationService = paginationService;
        _activationRepository = activationRepository;
        _authRepository = authRepository;
        _navigationService = navigationService;
        _mockSettingsRepository = new MockSettingsRepository();
    }

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    /// <summary>
    /// Open the Upgrade Pro URL in browser
    /// </summary>
    [RelayCommand]
    private async Task OpenUpgradeProAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(UpgradeProUrl))
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(UpgradeProUrl));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsViewModel] Failed to open upgrade URL: {ex.Message}");
        }
    }

    /// <summary>
    /// Activate license code (supports both trial and permanent codes)
    /// </summary>
    [RelayCommand]
    private async Task ActivateTrialCodeAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TrialCode))
            {
                await _toastHelper.ShowWarning("Please enter an activation code.");
                return;
            }

            // Get current user
            var userResult = await _authRepository.GetCurrentUserAsync();
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                await _toastHelper.ShowError("Could not get current user. Please login again.");
                return;
            }

            var currentUser = userResult.Data;
            var code = TrialCode.Trim().ToUpperInvariant();

            // Validate code first
            var validateResult = await _activationRepository.ValidateCodeAsync(code);
            if (!validateResult.IsSuccess || validateResult.Data == null)
            {
                await _toastHelper.ShowError(validateResult.ErrorMessage ?? "Invalid or already used activation code.");
                return;
            }

            var codeInfo = validateResult.Data;

            // Activate the code
            var activateResult = await _activationRepository.ActivateCodeAsync(code, currentUser.Id);
            if (activateResult.IsSuccess && activateResult.Data != null)
            {
                var license = activateResult.Data;
                TrialCode = string.Empty;
                
                if (license.IsPermanent)
                {
                    // Permanent license activated
                    IsPermanentAdmin = true;
                    IsTrialAdmin = false;
                    ShowTrialTab = false;
                    LicenseType = "Permanent";
                    LicenseExpiresAt = null;
                    TrialDaysRemaining = 0;
                    await _toastHelper.ShowSuccess("🎉 Permanent license activated! You now have unlimited access.");
                    Debug.WriteLine($"[SettingsViewModel] Permanent license activated for user: {currentUser.Id}");
                    
                    // Navigate to Dashboard within shell
                    await _navigationService.NavigateInShell("MyShop.Client.Views.Admin.AdminDashboardPage", currentUser);
                }
                else
                {
                    // Trial license extended
                    TrialDaysRemaining = license.RemainingDays;
                    LicenseExpiresAt = license.ExpiresAt;
                    LicenseType = "Trial";
                    IsTrialAdmin = true;
                    IsPermanentAdmin = false;
                    ShowTrialTab = true;
                    await _toastHelper.ShowSuccess($"Trial extended! {license.RemainingDays} days remaining.");
                    Debug.WriteLine($"[SettingsViewModel] Trial activated: {license.RemainingDays} days remaining");
                    
                    // Reload license info to ensure UI is fully updated
                    await LoadLicenseInfoAsync();
                }
            }
            else
            {
                await _toastHelper.ShowError(activateResult.ErrorMessage ?? "Failed to activate code.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsViewModel] Failed to activate code: {ex.Message}");
            await _toastHelper.ShowError("Failed to activate code.");
        }
    }

    /// <summary>
    /// Load trial/system settings from mock repository
    /// </summary>
    private async Task LoadSystemSettingsAsync()
    {
        try
        {
            var systemSettings = await _mockSettingsRepository.GetSystemSettingsAsync();
            if (systemSettings != null)
            {
                UpgradeProUrl = systemSettings.UpgradeProUrl;
                SupportUrl = systemSettings.SupportUrl;
                Debug.WriteLine($"[SettingsViewModel] System settings loaded: UpgradeUrl={UpgradeProUrl}");
            }
            
            // Load license info for Trial tab visibility
            await LoadLicenseInfoAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsViewModel] Failed to load system settings: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Load license info to determine Trial tab visibility
    /// Only show Trial tab for Admin users with Trial license
    /// </summary>
    private async Task LoadLicenseInfoAsync()
    {
        try
        {
            var licenseResult = await _activationRepository.GetCurrentLicenseAsync();
            if (licenseResult.IsSuccess && licenseResult.Data != null)
            {
                var license = licenseResult.Data;
                IsAdmin = true;
                IsTrialAdmin = !license.IsPermanent;
                IsPermanentAdmin = license.IsPermanent;
                LicenseType = license.IsPermanent ? "Permanent" : "Trial";
                LicenseExpiresAt = license.ExpiresAt;
                TrialDaysRemaining = license.RemainingDays;
                
                // Only show Trial tab for Trial admin (not Permanent admin)
                ShowTrialTab = IsTrialAdmin;
                
                Debug.WriteLine($"[SettingsViewModel] License loaded: Type={LicenseType}, ExpiresAt={LicenseExpiresAt}, RemainingDays={TrialDaysRemaining}, ShowTrialTab={ShowTrialTab}");
            }
            else
            {
                // No license = not admin or no license activated
                IsAdmin = false;
                IsTrialAdmin = false;
                IsPermanentAdmin = false;
                ShowTrialTab = false;
                LicenseType = "None";
                LicenseExpiresAt = null;
                TrialDaysRemaining = 0;
                
                Debug.WriteLine("[SettingsViewModel] No license found - hiding Trial tab");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsViewModel] Failed to load license info: {ex.Message}");
            ShowTrialTab = false;
        }
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
            IsLoading = true;

            var savePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("JSON Settings", new List<string> { ".json" });
            savePicker.SuggestedFileName = $"myshop-settings-{DateTime.Now:yyyyMMdd-HHmmss}";

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                var result = await _settingsStorage.GetAsync();
                if (!result.IsSuccess || result.Data == null)
                {
                    await _toastHelper.ShowError("Failed to load current settings for export.");
                    return;
                }
                
                var settings = result.Data;
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                await FileIO.WriteTextAsync(file, json);

                await _toastHelper.ShowSuccess($"Settings exported to {file.Name}");
                System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Settings exported to: {file.Path}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Export error: {ex.Message}");
            await _toastHelper.ShowError("Failed to export settings.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ImportSettingsAsync()
    {
        try
        {
            IsLoading = true;

            var openPicker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".json");

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var json = await FileIO.ReadTextAsync(file);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (settings != null)
                {
                    // Validate settings before applying
                    if (ValidateImportedSettings(settings))
                    {
                        var result = await _settingsStorage.SaveAsync(settings);
                        if (result.IsSuccess)
                        {
                            await LoadAsync();
                            await _toastHelper.ShowSuccess($"Settings imported from {file.Name}");
                            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Settings imported from: {file.Path}");
                        }
                        else
                        {
                            await _toastHelper.ShowError("Failed to save imported settings.");
                        }
                    }
                    else
                    {
                        await _toastHelper.ShowError("Invalid settings file format.");
                    }
                }
                else
                {
                    await _toastHelper.ShowError("Failed to parse settings file.");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Import error: {ex.Message}");
            await _toastHelper.ShowError("Failed to import settings.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool ValidateImportedSettings(AppSettings settings)
    {
        // Basic validation
        if (settings == null) return false;
        
        // Validate theme
        if (!string.IsNullOrEmpty(settings.Theme) && 
            !new[] { "System", "Light", "Dark" }.Contains(settings.Theme))
        {
            return false;
        }

        // Validate page sizes via Pagination object
        if (settings.Pagination != null)
        {
            if (settings.Pagination.ProductsPageSize < 1 || settings.Pagination.ProductsPageSize > 200) return false;
            if (settings.Pagination.OrdersPageSize < 1 || settings.Pagination.OrdersPageSize > 200) return false;
            if (settings.Pagination.CustomersPageSize < 1 || settings.Pagination.CustomersPageSize > 200) return false;
        }

        return true;
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

            // Load system settings (trial info, upgrade URL, etc.)
            await LoadSystemSettingsAsync();

            var result = await _settingsStorage.GetAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to load settings.";
                return;
            }

            var settings = result.Data;
            
            Theme = settings.Theme;
            Language = settings.Language;
            NotificationsEnabled = settings.NotificationsEnabled;
            RestoreLastPage = settings.RestoreLastPage;
            ProductsPageSize = settings.Pagination.ProductsPageSize;
            OrdersPageSize = settings.Pagination.OrdersPageSize;
            CustomersPageSize = settings.Pagination.CustomersPageSize;
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
                Pagination = new PaginationSettings
                {
                    ProductsPageSize = ProductsPageSize,
                    OrdersPageSize = OrdersPageSize,
                    CustomersPageSize = CustomersPageSize
                },
                EnableSoundNotifications = EnableSoundNotifications,
                NotifyOnLowStock = NotifyOnLowStock,
                NotifyOnNewOrders = NotifyOnNewOrders,
                EnableOfflineMode = EnableOfflineMode,
                ShopName = ShopName,
                Address = Address,
                SelectedTimezoneIndex = SelectedTimezoneIndex
            };

            var result = await _settingsStorage.SaveAsync(settings);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to save settings.";
                return;
            }

            // Update original settings
            _originalSettings = settings;
            
            // Sync with global PaginationService so all ViewModels get updated values
            _paginationService.Initialize(settings.Pagination);
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] PaginationService synced: Products={ProductsPageSize}, Orders={OrdersPageSize}");

            await _toastHelper.ShowSuccess("Settings saved successfully!");
            
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

            var result = await _settingsStorage.ResetAsync();
            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to reset settings.";
                return;
            }
            
            // Reload defaults
            await LoadAsync();

            await _toastHelper.ShowInfo("Settings reset to defaults.");
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
               ProductsPageSize != _originalSettings.Pagination.ProductsPageSize ||
               OrdersPageSize != _originalSettings.Pagination.OrdersPageSize ||
               CustomersPageSize != _originalSettings.Pagination.CustomersPageSize ||
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
    /// Only applies if there are actual changes to theme/language
    /// </summary>
    private void ApplyThemeAndLanguage()
    {
        try
        {
            // Only apply theme if it actually changed
            if (_originalSettings != null && Theme == _originalSettings.Theme)
            {
                System.Diagnostics.Debug.WriteLine("[SettingsViewModel] Theme unchanged, skipping apply");
                // Still check language change
                if (Language != _originalSettings.Language)
                {
                    System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Language changed to: {Language}");
                    _ = _toastHelper.ShowInfo("Language changes will take effect after app restart.");
                }
                return;
            }
            
            // Apply theme
            if (App.MainWindow == null)
            {
                System.Diagnostics.Debug.WriteLine("[SettingsViewModel] App.MainWindow is null, cannot apply theme");
                return;
            }

            if (App.MainWindow.Content is not Microsoft.UI.Xaml.FrameworkElement root)
            {
                System.Diagnostics.Debug.WriteLine("[SettingsViewModel] App.MainWindow.Content is not a FrameworkElement");
                return;
            }

            var requestedTheme = Theme switch
            {
                "Light" => Microsoft.UI.Xaml.ElementTheme.Light,
                "Dark" => Microsoft.UI.Xaml.ElementTheme.Dark,
                "System" => Microsoft.UI.Xaml.ElementTheme.Default,
                _ => Microsoft.UI.Xaml.ElementTheme.Default
            };

            // Dispatch to UI thread to avoid threading issues
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    root.RequestedTheme = requestedTheme;
                    System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Applied theme: {Theme} -> {requestedTheme}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Theme application error: {ex.Message}");
                }
            });

            // Apply language (requires app restart for full effect)
            if (_originalSettings != null && Language != _originalSettings.Language)
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Language changed to: {Language}");
                _ = _toastHelper.ShowInfo("Language changes will take effect after app restart.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] ApplyThemeAndLanguage error: {ex.Message}");
        }
    }
}
