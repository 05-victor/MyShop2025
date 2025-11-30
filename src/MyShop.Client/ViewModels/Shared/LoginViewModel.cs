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
using MyShop.Client.Strategies;

namespace MyShop.Client.ViewModels.Shared;

/// <summary>
/// LoginViewModel - Refactored to use Facade Pattern
/// Before: Injected 5 services (IMediator, INavigationService, IToastService, IRoleStrategyFactory, IValidationService)
/// After: Injected 3 dependencies (IAuthFacade aggregates auth operations, plus navigation and strategy)
/// Benefits: Simplified dependencies, centralized auth logic, easier testing
/// </summary>
public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthFacade _authFacade;
    private new readonly INavigationService _navigationService;
    private readonly IRoleStrategyFactory _roleStrategyFactory;
    private new readonly IToastService _toastHelper;
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
    /// Kiểm tra username có hợp lệ không
    /// </summary>
    public bool IsUsernameValid => string.IsNullOrWhiteSpace(UsernameError);

    /// <summary>
    /// Kiểm tra password có hợp lệ không
    /// </summary>
    public bool IsPasswordValid => string.IsNullOrWhiteSpace(PasswordError);

    /// <summary>
    /// Kiểm tra form có hợp lệ không (để enable/disable nút Login)
    /// </summary>
    public bool IsFormValid => 
        IsUsernameValid && 
        IsPasswordValid && 
        !string.IsNullOrWhiteSpace(Username) && 
        !string.IsNullOrWhiteSpace(Password);

    /// <summary>
    /// Property for button binding (similar to RegisterViewModel.CanRegister)
    /// </summary>
    public bool CanLogin => IsFormValid && !IsLoading;

    public string LoginButtonText => IsLoading ? "Signing in..." : "Sign In";

    public LoginViewModel(
        IAuthFacade authFacade,
        INavigationService navigationService,
        IRoleStrategyFactory roleStrategyFactory,
        IToastService toastHelper)
    {
        _authFacade = authFacade ?? throw new ArgumentNullException(nameof(authFacade));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _roleStrategyFactory = roleStrategyFactory ?? throw new ArgumentNullException(nameof(roleStrategyFactory));
        _toastHelper = toastHelper ?? throw new ArgumentNullException(nameof(toastHelper));

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
    /// Real-time validation khi username thay đổi
    /// Validation logic now encapsulated in AuthFacade
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
    /// Real-time validation khi password thay đổi
    /// Validation logic now encapsulated in AuthFacade
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
    /// Kiểm tra xem có thể attempt login không
    /// </summary>
    private bool CanAttemptLogin() => IsFormValid && !IsLoading;

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

            // ===== USE FACADE PATTERN =====
            // AuthFacade handles: Validation → Login → Storage → Toast notification
            // ViewModel chỉ cần gọi 1 method thay vì orchestrate 5+ services
            var result = await _authFacade.LoginAsync(Username, Password, IsRememberMe);

            if (result.IsSuccess && result.Data != null)
            {
                var user = result.Data;

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
                    // AuthFacade đã validate và return user-friendly error message
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
        await _toastHelper.ShowInfo("Password recovery feature coming soon!");
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
