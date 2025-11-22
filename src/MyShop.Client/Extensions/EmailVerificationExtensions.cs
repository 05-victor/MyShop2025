using Microsoft.UI.Xaml;
using MyShop.Client.Views.Dialogs;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.Extensions
{
    /// <summary>
    /// Extension methods for email verification workflow
    /// </summary>
    public static class EmailVerificationExtensions
    {
        /// <summary>
        /// Show email verification dialog and handle verification flow
        /// </summary>
        /// <param name="user">Current user</param>
        /// <param name="authRepository">Auth repository for verification</param>
        /// <param name="toastHelper">Toast helper for notifications</param>
        /// <param name="xamlRoot">XamlRoot for dialog</param>
        /// <returns>True if verified, false if not verified or cancelled</returns>
        public static async Task<bool> ShowEmailVerificationDialogAsync(
            this User user,
            IAuthRepository authRepository,
            IToastService toastHelper,
            XamlRoot xamlRoot)
        {
            if (user.IsEmailVerified)
            {
                return true; // Already verified
            }

            var dialog = new EmailVerificationDialog(user.Email, toastHelper)
            {
                XamlRoot = xamlRoot
            };

            bool isVerified = false;

            // Handle verification check event
            dialog.VerificationChecked += async (sender, verified) =>
            {
                if (verified)
                {
                    // Update user model
                    user.IsEmailVerified = true;
                    isVerified = true;
                }
            };

            await dialog.ShowAsync();

            return isVerified;
        }

        /// <summary>
        /// Check if email verification is required before action
        /// If not verified, show dialog
        /// </summary>
        public static async Task<bool> RequireEmailVerificationAsync(
            this User user,
            IAuthRepository authRepository,
            IToastService toastHelper,
            XamlRoot xamlRoot,
            string actionName = "use this feature")
        {
            if (user.IsEmailVerified)
            {
                return true; // Verified, can proceed
            }

            // Show blocking message
            toastHelper.ShowWarning($"Email verification required to {actionName}");

            // Show verification dialog
            var verified = await user.ShowEmailVerificationDialogAsync(
                authRepository, 
                toastHelper, 
                xamlRoot);

            if (!verified)
            {
                toastHelper.ShowInfo("Action cancelled - email not verified");
            }

            return verified;
        }
    }
}
