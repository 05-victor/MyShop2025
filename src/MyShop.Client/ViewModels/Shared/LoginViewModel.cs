using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.Features.Auth.Commands;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Shared;
using MyShop.Client.Views.Dialogs;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

// ===== NEW NAMESPACES - After Refactor =====
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Strategies;

namespace MyShop.Client.ViewModels.Shared;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastHelper;
    private readonly IRoleStrategyFactory _roleStrategyFactory;
    private readonly IValidationService _validationService;
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

    public string LoginButtonText => IsLoading ? "Signing in..." : "Sign In";

    public LoginViewModel(
        IMediator mediator,
        INavigationService navigationService,
        IToastService toastHelper,
        IRoleStrategyFactory roleStrategyFactory,
        IValidationService validationService)
    {
        _mediator = mediator;
        _navigationService = navigationService;
        _toastHelper = toastHelper;
        _roleStrategyFactory = roleStrategyFactory;
        _validationService = validationService;

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
    /// </summary>
    partial void OnUsernameChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var result = _validationService.ValidateUsername(value);
            UsernameError = result.IsValid ? string.Empty : result.ErrorMessage;
        }
        else
        {
            UsernameError = string.Empty;
        }
    }

    /// <summary>
    /// Real-time validation khi password thay đổi
    /// </summary>
    partial void OnPasswordChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var result = _validationService.ValidatePassword(value);
            PasswordError = result.IsValid ? string.Empty : result.ErrorMessage;
        }
        else
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

            // Validation
            if (!ValidateInput())
            {
                return;
            }

            // Hiện loading overlay ngay lập tức
            SetLoadingState(true);

            // Use MediatR to send LoginCommand
            var command = new LoginCommand(Username, Password, IsRememberMe);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess && result.Data != null)
            {
                var user = result.Data;

                // Show success message
                _toastHelper.ShowSuccess($"Welcome back, {user.Username}!");

                // Use strategy pattern để navigate đến đúng dashboard
                var primaryRole = user.GetPrimaryRole();
                var strategy = _roleStrategyFactory.GetStrategy(primaryRole);
                var pageType = strategy.GetDashboardPageType();
                    
                _navigationService.NavigateTo(pageType.FullName!, user);
            }
            else
            {
                // Nếu lỗi mạng (repository đã bắt và đính kèm Exception), hiển thị dialog kết nối
                if (result.Exception is HttpRequestException || result.Exception is SocketException || result.Exception is TaskCanceledException)
                {
                    await HandleConnectionErrorAsync();
                }
                else
                {
                    // Repository đã handle hết exceptions và map thành error message
                    SetError(result.ErrorMessage ?? "Login failed. Please try again.");
                }
            }
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"Network Error: {httpEx.Message}");
            // Show immediate inline error to guarantee UX baseline
            SetError("Cannot connect to server. Please check your network connection and ensure the server is running.");
            // Then offer richer UX dialog if possible
            await HandleConnectionErrorAsync();
        }
        catch (System.Exception ex)
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

        var action = await _toastHelper.ShowConnectionErrorAsync(
            "Cannot connect to server. Please check your network connection and ensure the server is running.");

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

    /// <summary>
    /// Validate input trước khi submit (sử dụng ValidationService)
    /// </summary>
    private bool ValidateInput()
    {
        bool isValid = true;

        // Validate username using validation service
        var usernameValidation = _validationService.ValidateUsername(Username);
        if (!usernameValidation.IsValid)
        {
            UsernameError = usernameValidation.ErrorMessage;
            isValid = false;
        }

        // Validate password using validation service
        var passwordValidation = _validationService.ValidatePassword(Password);
        if (!passwordValidation.IsValid)
        {
            PasswordError = passwordValidation.ErrorMessage;
            isValid = false;
        }

        return isValid;
    }

    [RelayCommand]
    private void NavigateToRegister()
    {
        _navigationService.NavigateTo(typeof(RegisterPage).FullName!);
    }

    [RelayCommand]
    private void ForgotPassword()
    {
        _toastHelper.ShowInfo("Password recovery feature coming soon!");
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

            await Task.Delay(1000); // Simulate network delay
            _toastHelper.ShowWarning("Google OAuth2 login will be implemented in a future update. Please use username/password login.");
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
