using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.Views.Dialogs
{
    public sealed partial class EmailVerificationDialog : ContentDialog
    {
        private readonly IToastService _toastHelper;
        private readonly IAuthRepository _authRepository;
        private DispatcherTimer? _countdownTimer;
        private int _countdownSeconds = 60;
        private bool _isResending = false;
        private string _userId = string.Empty;

        public string UserEmail { get; set; } = string.Empty;
        public event EventHandler<bool>? VerificationChecked;

        public EmailVerificationDialog()
        {
            this.InitializeComponent();
            
            // Get services from DI
            _toastHelper = App.Current.Services.GetRequiredService<IToastService>();
            _authRepository = App.Current.Services.GetRequiredService<IAuthRepository>();
        }

        public EmailVerificationDialog(string email, IToastService toastHelper) : this()
        {
            UserEmail = email;
            _toastHelper = toastHelper;
            EmailText.Text = email;
            
            // Send initial verification email
            _ = SendVerificationEmailAsync();
        }

        private async Task SendVerificationEmailAsync()
        {
            try
            {
                // Get current user ID
                var userIdResult = await _authRepository.GetCurrentUserIdAsync();
                if (userIdResult.IsSuccess)
                {
                    _userId = userIdResult.Data.ToString();
                    
                    // Send verification email
                    var result = await _authRepository.SendVerificationEmailAsync(_userId);
                    if (result.IsSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine($"[EmailVerificationDialog] Verification code sent to {UserEmail}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EmailVerificationDialog] Error sending verification: {ex.Message}");
            }
        }

        private async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isResending) return;

            try
            {
                _isResending = true;
                ResendButton.IsEnabled = false;
                ErrorMessage.Visibility = Visibility.Collapsed;
                
                // Show loading state
                ResendIcon.Glyph = "\uE895"; // Loading icon
                ResendText.Text = "Sending...";

                // Call repository to resend verification email
                var result = await _authRepository.SendVerificationEmailAsync(_userId);

                if (result.IsSuccess)
                {
                    // Show success message
                    SuccessMessage.Visibility = Visibility.Visible;
                    _toastHelper.ShowSuccess($"Verification code sent to {UserEmail}");

                    // Hide success message after 3 seconds
                    await Task.Delay(3000);
                    SuccessMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _toastHelper.ShowError(result.ErrorMessage ?? "Failed to send verification code");
                }

                // Reset button state
                ResendIcon.Glyph = "\uE72C"; // Refresh icon
                ResendText.Text = "Resend Code";

                // Start countdown
                StartCountdown();
            }
            catch (Exception ex)
            {
                _toastHelper.ShowError("Failed to send code. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Error resending email: {ex.Message}");
                
                // Reset button
                ResendButton.IsEnabled = true;
                ResendIcon.Glyph = "\uE72C";
                ResendText.Text = "Resend Code";
            }
            finally
            {
                _isResending = false;
            }
        }

        private void StartCountdown()
        {
            _countdownSeconds = 60;
            CountdownText.Visibility = Visibility.Visible;
            CountdownText.Text = $"You can resend in {_countdownSeconds} seconds";

            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(1);
            _countdownTimer.Tick += (s, e) =>
            {
                _countdownSeconds--;
                
                if (_countdownSeconds <= 0)
                {
                    _countdownTimer?.Stop();
                    CountdownText.Visibility = Visibility.Collapsed;
                    ResendButton.IsEnabled = true;
                }
                else
                {
                    CountdownText.Text = $"You can resend in {_countdownSeconds} seconds";
                }
            };
            _countdownTimer.Start();
        }

        private async void PrimaryButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Defer to show loading state
            var deferral = args.GetDeferral();

            try
            {
                var otpCode = OtpInput.Text?.Trim() ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(otpCode))
                {
                    args.Cancel = true;
                    ErrorText.Text = "Please enter the verification code.";
                    ErrorMessage.Visibility = Visibility.Visible;
                    deferral.Complete();
                    return;
                }

                if (otpCode.Length != 6 || !System.Text.RegularExpressions.Regex.IsMatch(otpCode, @"^\d{6}$"))
                {
                    args.Cancel = true;
                    ErrorText.Text = "Please enter a valid 6-digit code.";
                    ErrorMessage.Visibility = Visibility.Visible;
                    deferral.Complete();
                    return;
                }

                // Call repository to verify OTP
                var result = await _authRepository.VerifyEmailAsync(_userId, otpCode);

                if (result.IsSuccess)
                {
                    _toastHelper.ShowSuccess("Email verified successfully!");
                    VerificationChecked?.Invoke(this, true);
                    // Dialog will close
                }
                else
                {
                    args.Cancel = true; // Prevent dialog close
                    ErrorText.Text = result.ErrorMessage ?? "Invalid verification code. Please try again.";
                    ErrorMessage.Visibility = Visibility.Visible;
                    OtpInput.Text = string.Empty; // Clear invalid code
                    OtpInput.Focus(FocusState.Programmatic);
                }
            }
            catch (Exception ex)
            {
                args.Cancel = true;
                ErrorText.Text = "Failed to verify code. Please try again.";
                ErrorMessage.Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine($"Error verifying code: {ex.Message}");
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void SecondaryButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Just close dialog
            _countdownTimer?.Stop();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            // Stop timer when dialog is closed
            this.Closed += (s, e) => _countdownTimer?.Stop();
        }
    }
}
