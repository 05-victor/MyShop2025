using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Shared;
using MyShop.Client.Views.Dialogs;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Client.Services;
using MyShop.Plugins.Infrastructure;
using MyShop.Client.Strategies;

namespace MyShop.Client.ViewModels.Shared;

/// <summary>
/// ViewModel for the Login page. Refactored to use Facade Pattern.
/// Before refactoring: Injected 5 services (IMediator, INavigationService, IToastService, IRoleStrategyFactory, IValidationService).
/// After refactoring: Injected 3 dependencies (IAuthFacade aggregates auth operations, plus navigation and strategy).
/// Benefits: Simplified dependencies, centralized auth logic, easier testing.
/// </summary>
public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthFacade _authFacade;
    private new readonly INavigationService _navigationService;
    private readonly IRoleStrategyFactory _roleStrategyFactory;
    private new readonly IToastService _toastHelper;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ISettingsStorage _settingsStorage;
    private CancellationTokenSource? _loginCancellationTokenSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUsernameValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptLoginCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptLoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isRememberMe = true;

    [ObservableProperty]
    private string _usernameError = string.Empty;

    [ObservableProperty]
    private string _passwordError = string.Empty;

    /// <summary>
    /// Check if username is valid (no error message).
    /// </summary>
    public bool IsUsernameValid => string.IsNullOrWhiteSpace(UsernameError);

    /// <summary>
    /// Check if password is valid (no error message).
    /// </summary>
    public bool IsPasswordValid => string.IsNullOrWhiteSpace(PasswordError);

    /// <summary>
    /// Check if form is valid (to enable/disable Login button).
    /// </summary>
    public bool IsFormValid =>
        IsUsernameValid &&
        IsPasswordValid &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password);

    /// <summary>
    /// Property for button binding (similar to RegisterViewModel.CanRegister).
    /// </summary>
    public bool CanLogin => IsFormValid && !IsLoading;

    /// <summary>
    /// Dynamic button text based on loading state.
    /// </summary>
    public string LoginButtonText => IsLoading ? "Signing in..." : "Sign In";

    public LoginViewModel(
        IAuthFacade authFacade,
        INavigationService navigationService,
        IRoleStrategyFactory roleStrategyFactory,
        IToastService toastHelper,
        ISettingsRepository settingsRepository,
        ISettingsStorage settingsStorage)
    {
        _authFacade = authFacade ?? throw new ArgumentNullException(nameof(authFacade));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _roleStrategyFactory = roleStrategyFactory ?? throw new ArgumentNullException(nameof(roleStrategyFactory));
        _toastHelper = toastHelper ?? throw new ArgumentNullException(nameof(toastHelper));
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _settingsStorage = settingsStorage ?? throw new ArgumentNullException(nameof(settingsStorage));

        // Notify LoginButtonText when IsLoading changes
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IsLoading))
            {
                OnPropertyChanged(nameof(LoginButtonText));
            }
        };
    }

    /// <summary>
    /// Real-time validation when username changes.
    /// Validation logic is now encapsulated in AuthFacade.
    /// </summary>
    partial void OnUsernameChanged(string value)
    {
        // Clear error when user types
        if (!string.IsNullOrWhiteSpace(value))
        {
            UsernameError = string.Empty;
        }
    }

    /// <summary>
    /// Real-time validation when password changes.
    /// Validation logic is now encapsulated in AuthFacade.
    /// </summary>
    partial void OnPasswordChanged(string value)
    {
        // Clear error when user types
        if (!string.IsNullOrWhiteSpace(value))
        {
            PasswordError = string.Empty;
        }
    }

    /// <summary>
    /// Check if login can be attempted.
    /// Uses IsFormValid and IsLoading to determine button state.
    /// </summary>
    private bool CanAttemptLogin() => IsFormValid && !IsLoading;

    /// <summary>
    /// Attempts to log in with the provided credentials.
    /// Uses Facade Pattern: AuthFacade handles Validation → Login → Storage → Toast notification.
    /// ViewModel only needs to call 1 method instead of orchestrating 5+ services.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAttemptLogin), IncludeCancelCommand = true)]
    private async Task AttemptLoginAsync(CancellationToken cancellationToken)
    {
        // Cancel previous login attempt if still running
        _loginCancellationTokenSource?.Cancel();
        _loginCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            // Clear previous errors
            ClearError();
            UsernameError = string.Empty;
            PasswordError = string.Empty;

            // Show loading overlay
            SetLoadingState(true);

            // AuthFacade handles: Validation → Login → Storage → Toast notification
            var result = await _authFacade.LoginAsync(Username, Password, IsRememberMe);

            if (result.IsSuccess && result.Data != null)
            {
                var user = result.Data;

                // Set current user for per-user settings storage (enables users/{UserId}/preferences.json)
                if (_settingsStorage is FileSettingsStorage fileStorage)
                {
                    fileStorage.SetCurrentUser(user.Id.ToString());
                }

                // Load settings from server (source of truth) and apply theme immediately
                System.Diagnostics.Debug.WriteLine("[LoginViewModel] Login successful, loading and applying user settings");
                var settingsResult = await _settingsRepository.GetSettingsAsync();
                if (settingsResult.IsSuccess && settingsResult.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine("[LoginViewModel] Settings loaded successfully");

                    // Apply user's theme preference to override session theme
                    if (!string.IsNullOrEmpty(settingsResult.Data.Theme))
                    {
                        var mappedTheme = ThemeMapping.FromAppSettings(settingsResult.Data.Theme);
                        ThemeManager.ApplyTheme(mappedTheme);
                        System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Applied user theme: {settingsResult.Data.Theme}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Warning: Failed to load settings: {settingsResult.ErrorMessage}");
                    // Don't block login even if settings failed to load - theme from session will be retained
                }

                // Use strategy pattern to navigate to appropriate dashboard
                var primaryRole = user.GetPrimaryRole();
                var strategy = _roleStrategyFactory.GetStrategy(primaryRole);
                var pageType = strategy.GetDashboardPageType();

                await _navigationService.NavigateTo(pageType.FullName!, user);
            }
            else
            {
                // Handle login failure
                if (result.Exception is HttpRequestException || result.Exception is SocketException || result.Exception is TaskCanceledException)
                {
                    await HandleConnectionErrorAsync();
                }
                else
                {
                    // AuthFacade already validated and returned user-friendly error message
                    SetError(result.ErrorMessage ?? "Login failed. Please try again.");
                }
            }
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"Network Error: {httpEx.Message}");
            SetError("Cannot connect to server. Please check your network connection and ensure the server is running.");
            await HandleConnectionErrorAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error in LoginViewModel: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            SetError("An unexpected error occurred. Please try again.");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async Task HandleConnectionErrorAsync()
    {
        // Ensure a baseline inline message is visible
        SetError("Cannot connect to server. Please check your network connection and ensure the server is running.");

        var actionResult = await _toastHelper.ShowConnectionErrorAsync(
            "Cannot connect to server. Please check your network connection and ensure the server is running.");

        if (!actionResult.IsSuccess || actionResult.Data == ConnectionErrorAction.Cancel)
        {
            return;
        }

        var action = actionResult.Data;

        switch (action)
        {
            case ConnectionErrorAction.Retry:
                // Retry login
                await AttemptLoginAsync(CancellationToken.None);
                break;

            case ConnectionErrorAction.ConfigureServer:
                // Show server config dialog
                await ShowServerConfigDialogAsync();
                break;

            case ConnectionErrorAction.Cancel:
                // Keep the inline error message visible as a fallback
                break;
        }
    }

    private async Task ShowServerConfigDialogAsync()
    {
        try
        {
            var dialog = new ServerConfigDialog();

            // Get XamlRoot from current window
            var window = App.MainWindow;
            if (window?.Content != null)
            {
                dialog.XamlRoot = window.Content.XamlRoot;
                await dialog.ShowAsync();
                // Note: Dialog handles restart internally if config changed
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing server config dialog: {ex.Message}");
            SetError("Failed to open server configuration dialog.");
        }
    }

    // NOTE: Validation logic moved to AuthFacade.LoginAsync()
    // No longer needed in ViewModel - Facade handles all validation

    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        await _navigationService.NavigateTo(typeof(RegisterPage).FullName!);
    }

    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        await _navigationService.NavigateTo("MyShop.Client.Views.Auth.ForgotPasswordRequestPage");
    }

    [RelayCommand]
    private async Task GoogleLogin()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            System.Diagnostics.Debug.WriteLine("Google Login clicked");

            // Google OAuth2 implementation would involve:
            // 1. Redirect to Google OAuth consent screen
            // 2. Receive authorization code
            // 3. Exchange code for access token
            // 4. Send token to backend for verification
            // 5. Backend creates/finds user and returns JWT

            // For WinUI 3, use WebAuthenticationBroker or System.Net.Http.HttpClient
            // Example flow:
            /*
            var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                $"client_id={_googleClientId}&" +
                $"redirect_uri={_redirectUri}&" +
                $"response_type=code&" +
                $"scope=openid email profile";
            
            var authResult = await WebAuthenticationBroker.AuthenticateAsync(
                WebAuthenticationOptions.None,
                new Uri(googleAuthUrl),
                new Uri(_redirectUri));
            
            if (authResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                var code = ExtractCode(authResult.ResponseData);
                var loginResult = await _authRepository.LoginWithGoogleAsync(code);
                
                if (loginResult.IsSuccess)
                {
                    // Handle successful login
                }
            }
            */

            // await Task.Delay(1000); // Simulate network delay
            await _toastHelper.ShowWarning("Google OAuth2 login will be implemented in a future update. Please use username/password login.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Google Login error: {ex.Message}");
            ErrorMessage = $"Google login error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ConfigureServer()
    {
        await ShowServerConfigDialogAsync();
    }
}
