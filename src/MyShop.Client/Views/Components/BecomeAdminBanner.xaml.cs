using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using MyShop.Shared.Models.Enums;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.Views.Components
{
    public sealed partial class BecomeAdminBanner : UserControl
    {
        private const string VALID_ACTIVATION_CODE = "MYSHOP-ADMIN-2025";
        private const int MIN_CODE_LENGTH = 15;
        
        private readonly IToastService _toastHelper;
        private readonly ICredentialStorage _credentialStorage;
        
        private bool _isActivating = false;

        public event EventHandler<User>? ActivationSuccessful;

        public BecomeAdminBanner()
        {
            this.InitializeComponent();
            
            // Get services from DI
            _toastHelper = App.Current.Services.GetRequiredService<IToastService>();
            _credentialStorage = App.Current.Services.GetRequiredService<ICredentialStorage>();
        }

        private void ActivationCodeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var code = ActivationCodeInput.Text;
            var length = code.Length;

            // Update character counter
            CharCounterText.Text = $"{length}/25";

            // Clear error when typing
            ErrorMessage.Visibility = Visibility.Collapsed;

            // Enable button if code length >= 15
            BecomeAdminButton.IsEnabled = length >= MIN_CODE_LENGTH;
        }

        private async void BecomeAdminButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isActivating) return;

            var code = ActivationCodeInput.Text.Trim();

            // Validate length
            if (code.Length < MIN_CODE_LENGTH)
            {
                ShowError("Activation code must be at least 15 characters");
                return;
            }

            _isActivating = true;
            BecomeAdminButton.IsEnabled = false;

            try
            {
                // Show loading state
                ButtonText.Text = "Activating...";

                // Simulate API call
                await Task.Delay(1500);

                // Validate code (case-insensitive)
                if (code.Equals(VALID_ACTIVATION_CODE, StringComparison.OrdinalIgnoreCase))
                {
                    // Success!
                    await HandleSuccessfulActivation();
                }
                else
                {
                    // Invalid code
                    ShowError("Invalid activation code. Please check and try again.");
                    await _toastHelper.ShowError("Invalid activation code");
                    ButtonText.Text = "Become Admin →";
                    BecomeAdminButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during activation: {ex.Message}");
                ShowError("An error occurred. Please try again.");
                await _toastHelper.ShowError("Activation failed");
                ButtonText.Text = "Become Admin →";
                BecomeAdminButton.IsEnabled = true;
            }
            finally
            {
                _isActivating = false;
            }
        }

        private async Task HandleSuccessfulActivation()
        {
            try
            {
                // Update user role to Admin
                // Note: In real app, this would call backend API
                // For demo, we'll use localStorage to mark admin exists
                
                // Note: ICredentialStorage only supports tokens, not key-value pairs
                // This should be moved to ISettingsStorage in the future
                // _credentialStorage.SaveItem("hasAdmin", "true");
                System.Diagnostics.Debug.WriteLine("[BecomeAdminBanner] Admin flag should be saved");
                
                await _toastHelper.ShowSuccess("🎉 Welcome Admin! Your account has been upgraded. Redirecting...");

                // Wait for toast to show
                await Task.Delay(2000);

                // Trigger event for parent to handle (reload/navigate)
                ActivationSuccessful?.Invoke(this, null!);

                // In real app, parent would handle navigation to AdminDashboard
                // For now, show completion
                _toastHelper.ShowInfo("Please restart the app to see admin features");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling activation success: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}
