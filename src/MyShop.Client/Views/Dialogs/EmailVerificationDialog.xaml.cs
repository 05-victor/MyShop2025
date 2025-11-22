using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.Views.Dialogs
{
    public sealed partial class EmailVerificationDialog : ContentDialog
    {
        private readonly IToastService _toastHelper;
        private DispatcherTimer? _countdownTimer;
        private int _countdownSeconds = 60;
        private bool _isResending = false;

        public string UserEmail { get; set; } = string.Empty;
        public event EventHandler<bool>? VerificationChecked;

        public EmailVerificationDialog()
        {
            this.InitializeComponent();
            
            // Get ToastHelper from DI
            _toastHelper = App.Current.Services.GetRequiredService<IToastService>();
        }

        public EmailVerificationDialog(string email, IToastService toastHelper) : this()
        {
            UserEmail = email;
            _toastHelper = toastHelper;
            EmailText.Text = email;
        }

        private async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isResending) return;

            try
            {
                _isResending = true;
                ResendButton.IsEnabled = false;
                
                // Show loading state
                ResendIcon.Glyph = "\uE895"; // Loading icon
                ResendText.Text = "Sending...";

                // Simulate API call (replace with actual repository call)
                await Task.Delay(1500);

                // Show success message
                SuccessMessage.Visibility = Visibility.Visible;
                _toastHelper.ShowSuccess($"✅ Email sent! Check your inbox - Verification email sent to {UserEmail}");

                // Hide success message after 3 seconds
                await Task.Delay(3000);
                SuccessMessage.Visibility = Visibility.Collapsed;

                // Reset button state
                ResendIcon.Glyph = "\uE72C"; // Refresh icon
                ResendText.Text = "Resend Email";

                // Start countdown
                StartCountdown();
            }
            catch (Exception ex)
            {
                _toastHelper.ShowError("Failed to send email. Please try again.");
                System.Diagnostics.Debug.WriteLine($"Error resending email: {ex.Message}");
                
                // Reset button
                ResendButton.IsEnabled = true;
                ResendIcon.Glyph = "\uE72C";
                ResendText.Text = "Resend Email";
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
                // Check verification status from backend
                var isVerified = await CheckVerificationStatusAsync();

                if (isVerified)
                {
                    _toastHelper.ShowSuccess("✅ Email verified successfully!");
                    VerificationChecked?.Invoke(this, true);
                }
                else
                {
                    args.Cancel = true; // Prevent dialog close
                    _toastHelper.ShowInfo("Please click the link in your email first - Verification may take a few moments to process");
                }
            }
            catch (Exception ex)
            {
                args.Cancel = true;
                _toastHelper.ShowError("Failed to check verification status");
                System.Diagnostics.Debug.WriteLine($"Error checking verification: {ex.Message}");
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

        private async Task<bool> CheckVerificationStatusAsync()
        {
            // Call profile repository to check email verification status
            // This assumes IProfileRepository has a method to get current user profile
            // For now, return false until backend implements verification check endpoint
            await Task.CompletedTask;
            return false;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            // Stop timer when dialog is closed
            this.Closed += (s, e) => _countdownTimer?.Stop();
        }
    }
}
