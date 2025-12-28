using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Services;
using System;
using System.Threading.Tasks;
using Windows.System.Threading;

namespace MyShop.Client.ViewModels.Auth;

public partial class ForgotPasswordOtpViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private ThreadPoolTimer? _timer;
    private int _remainingSeconds = 300; // 5 minutes
    private int _resendCooldown = 0;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _otpCode = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _timerText = "Code expires in 05:00";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanVerify))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanResend))]
    private bool _resendDisabled = true;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool CanVerify => !IsLoading && OtpCode.Length == 6;

    public bool CanResend => !IsLoading && !ResendDisabled;

    public string MaskedEmail
    {
        get
        {
            if (string.IsNullOrEmpty(Email))
                return string.Empty;

            var parts = Email.Split('@');
            if (parts.Length != 2)
                return Email;

            var username = parts[0];
            var domain = parts[1];
            
            // Mask username: show first and last char, mask middle
            var masked = username.Length <= 2 
                ? new string('*', username.Length)
                : $"{username[0]}{new string('*', username.Length - 2)}{username[^1]}";

            return $"{masked}@{domain}";
        }
    }

    public ForgotPasswordOtpViewModel(
        IAuthService authService,
        INavigationService navigationService,
        IToastService toastService)
        : base(toastService, navigationService)
    {
        _authService = authService;
    }

    public void InitializeWithEmail(string email)
    {
        Email = email;
        OnPropertyChanged(nameof(MaskedEmail));
        StartTimer();
    }

    partial void OnOtpCodeChanged(string value)
    {
        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(CanVerify));
    }

    [RelayCommand]
    private async Task VerifyAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (OtpCode.Length != 6)
            {
                ErrorMessage = "Please enter complete 6-digit code";
                return;
            }

            IsLoading = true;

            // Call API to verify OTP
            var result = await _authService.VerifyPasswordResetCodeAsync(Email, OtpCode);

            if (result.IsSuccess)
            {
                StopTimer();
                
                // Navigate to reset password page with email and token
                _navigationService?.NavigateTo("ForgotPasswordReset", new { Email, Token = OtpCode });
            }
            else
            {
                // Handle specific errors
                ErrorMessage = result.ErrorMessage switch
                {
                    var msg when msg?.Contains("invalid", StringComparison.OrdinalIgnoreCase) == true
                        => "Invalid code",
                    var msg when msg?.Contains("expired", StringComparison.OrdinalIgnoreCase) == true
                        => "Code expired",
                    var msg when msg?.Contains("too many", StringComparison.OrdinalIgnoreCase) == true
                        => "Too many attempts. Try again later.",
                    _ => result.ErrorMessage ?? "Verification failed"
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ForgotPasswordOtpViewModel] Error: {ex.Message}");
            ErrorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ResendCodeAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            var result = await _authService.SendPasswordResetCodeAsync(Email);

            if (result.IsSuccess)
            {
                await _toastHelper.ShowSuccess("Verification code resent to your email");
                
                // Reset timer
                _remainingSeconds = 300;
                _resendCooldown = 30; // 30 second cooldown
                ResendDisabled = true;
                StartTimer();
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to resend code";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ForgotPasswordOtpViewModel] Resend error: {ex.Message}");
            ErrorMessage = "Failed to resend code";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ChangeEmail()
    {
        StopTimer();
        _navigationService?.NavigateTo("ForgotPasswordRequest");
    }

    [RelayCommand]
    private void Back()
    {
        StopTimer();
        _navigationService?.NavigateTo("ForgotPasswordRequest");
    }

    private void StartTimer()
    {
        _timer?.Cancel();
        
        _timer = ThreadPoolTimer.CreatePeriodicTimer((timer) =>
        {
            if (_remainingSeconds > 0)
            {
                _remainingSeconds--;
                
                // Update resend cooldown
                if (_resendCooldown > 0)
                {
                    _resendCooldown--;
                    if (_resendCooldown == 0)
                    {
                        _ = App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                        {
                            ResendDisabled = false;
                        });
                    }
                }

                // Update timer text on UI thread
                var minutes = _remainingSeconds / 60;
                var seconds = _remainingSeconds % 60;
                var timerText = $"Code expires in {minutes:D2}:{seconds:D2}";

                _ = App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    TimerText = timerText;
                });
            }
            else
            {
                // Timer expired
                _ = App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    TimerText = "Code expired";
                    ErrorMessage = "This code has expired. Please request a new one.";
                });
                timer.Cancel();
            }
        }, TimeSpan.FromSeconds(1));
    }

    private void StopTimer()
    {
        _timer?.Cancel();
        _timer = null;
    }
}
