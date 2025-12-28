using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Services.Configuration;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Plugins.Repositories.Mocks;
using MyShop.Shared.Models.Enums;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
// Use alias to avoid ambiguity with MockSettingsRepository.AppSettings
using AppSettings = MyShop.Shared.Models.AppSettings;

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

    // Shop information settings (Admin only)
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _shopName = "MyShop 2025";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _address = string.Empty;

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
    /// Trial tab shows for: Admin without Permanent license (Trial or No license)
    /// Trial tab hidden for: Non-Admin or Admin with Permanent license
    /// </summary>
    private async Task LoadLicenseInfoAsync()
    {
        try
        {
            // First check if user is Admin
            var userResult = await _authRepository.GetCurrentUserAsync();
            if (userResult.IsSuccess && userResult.Data != null)
            {
                var currentUser = userResult.Data;
                IsAdmin = currentUser.Roles != null && currentUser.Roles.Contains(UserRole.Admin);
            }
            else
            {
                IsAdmin = false;
            }

            // Only load license info if user is Admin
            if (!IsAdmin)
            {
                ShowTrialTab = false;
                Debug.WriteLine("[SettingsViewModel] User is not Admin - hiding Trial tab");
                return;
            }

            var licenseResult = await _activationRepository.GetCurrentLicenseAsync();
            if (licenseResult.IsSuccess && licenseResult.Data != null)
            {
                var license = licenseResult.Data;
                IsTrialAdmin = !license.IsPermanent;
                IsPermanentAdmin = license.IsPermanent;
                LicenseType = license.IsPermanent ? "Permanent" : "Trial";
                LicenseExpiresAt = license.ExpiresAt;
                TrialDaysRemaining = license.RemainingDays;
                
                // Show Trial tab only for Admin without Permanent license
                ShowTrialTab = !license.IsPermanent;
                
                Debug.WriteLine($"[SettingsViewModel] License loaded: Type={LicenseType}, IsPermanent={license.IsPermanent}, ShowTrialTab={ShowTrialTab}");
            }
            else
            {
                // Admin without license - show Trial tab so they can activate
                IsTrialAdmin = false;
                IsPermanentAdmin = false;
                ShowTrialTab = true;
                LicenseType = "None";
                LicenseExpiresAt = null;
                TrialDaysRemaining = 0;
                
                Debug.WriteLine($"[SettingsViewModel] Admin without license - showing Trial tab for activation");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsViewModel] Failed to load license info: {ex.Message}");
            ShowTrialTab = false;
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
            ShopName = settings.ShopName ?? "MyShop 2025";
            Address = settings.Address ?? string.Empty;

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
                ShopName = ShopName,
                Address = Address
            };

            var result = await _settingsStorage.SaveAsync(settings);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to save settings.";
                return;
            }

            // Update original settings
            _originalSettings = settings;
            
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
               ShopName != _originalSettings.ShopName ||
               Address != _originalSettings.Address;
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
