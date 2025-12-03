using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.Views.Components
{
    public sealed partial class BecomeAdminBanner : UserControl
    {
        private const int MIN_CODE_LENGTH = 18; // MYSHOP-XXXX-XXXX-XXXX = 22 chars, min 18
        
        private readonly IToastService _toastHelper;
        private readonly ICredentialStorage _credentialStorage;
        private readonly ISystemActivationRepository _activationRepository;
        
        private bool _isActivating = false;
        private Guid _currentUserId = Guid.Empty;

        public event EventHandler<LicenseInfo>? ActivationSuccessful;

        /// <summary>
        /// Set the current user ID for activation
        /// </summary>
        public Guid CurrentUserId
        {
            get => _currentUserId;
            set => _currentUserId = value;
        }

        public BecomeAdminBanner()
        {
            this.InitializeComponent();
            
            // Get services from DI
            _toastHelper = App.Current.Services.GetRequiredService<IToastService>();
            _credentialStorage = App.Current.Services.GetRequiredService<ICredentialStorage>();
            _activationRepository = App.Current.Services.GetRequiredService<ISystemActivationRepository>();
        }

        private void ActivationCodeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var code = ActivationCodeInput.Text;
            var length = code.Length;

            // Update character counter
            CharCounterText.Text = $"{length}/22";

            // Clear error when typing
            ErrorMessage.Visibility = Visibility.Collapsed;

            // Enable button if code length >= MIN_CODE_LENGTH
            BecomeAdminButton.IsEnabled = length >= MIN_CODE_LENGTH;
        }

        private async void BecomeAdminButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isActivating) return;

            var code = ActivationCodeInput.Text.Trim().ToUpperInvariant();

            // Validate length
            if (code.Length < MIN_CODE_LENGTH)
            {
                ShowError($"Activation code must be at least {MIN_CODE_LENGTH} characters");
                return;
            }

            // Validate user ID
            if (_currentUserId == Guid.Empty)
            {
                ShowError("User session not found. Please login again.");
                return;
            }

            _isActivating = true;
            BecomeAdminButton.IsEnabled = false;

            try
            {
                // Show loading state
                ButtonText.Text = "Activating...";

                // Validate code first
                var validateResult = await _activationRepository.ValidateCodeAsync(code);
                
                if (!validateResult.IsSuccess)
                {
                    ShowError(validateResult.ErrorMessage ?? "Invalid activation code");
                    await _toastHelper.ShowError("Invalid activation code");
                    ResetButton();
                    return;
                }

                // Activate the code
                var activateResult = await _activationRepository.ActivateCodeAsync(code, _currentUserId);

                if (activateResult.IsSuccess && activateResult.Data != null)
                {
                    await HandleSuccessfulActivation(activateResult.Data);
                }
                else
                {
                    ShowError(activateResult.ErrorMessage ?? "Activation failed");
                    await _toastHelper.ShowError(activateResult.ErrorMessage ?? "Activation failed");
                    ResetButton();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BecomeAdminBanner] Error during activation: {ex.Message}");
                ShowError("An error occurred. Please try again.");
                await _toastHelper.ShowError("Activation failed");
                ResetButton();
            }
            finally
            {
                _isActivating = false;
            }
        }

        private async Task HandleSuccessfulActivation(LicenseInfo license)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[BecomeAdminBanner] Activation successful: Type={license.Type}, ExpiresAt={license.ExpiresAt}");
                
                string message;
                if (license.IsPermanent)
                {
                    message = "🎉 Welcome Admin! You have permanent access.";
                }
                else
                {
                    message = $"🎉 Welcome Admin! Your trial is active for {license.RemainingDays} days.";
                }
                
                await _toastHelper.ShowSuccess(message);

                // Update button to show success
                ButtonText.Text = "✓ Activated!";
                BecomeAdminButton.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.LightGreen);

                // Trigger event for parent to handle (reload/navigate)
                ActivationSuccessful?.Invoke(this, license);

                // Show restart info
                await _toastHelper.ShowInfo("Please restart the app to access admin features");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BecomeAdminBanner] Error handling activation success: {ex.Message}");
            }
        }

        private void ResetButton()
        {
            ButtonText.Text = "Become Admin →";
            BecomeAdminButton.IsEnabled = ActivationCodeInput.Text.Length >= MIN_CODE_LENGTH;
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}
