using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyShop.Client.Facades;

/// <summary>
/// Implementation of IProfileFacade - aggregates profile-related services
/// </summary>
public class ProfileFacade : IProfileFacade
{
    private readonly IAuthRepository _authRepository;
    private readonly IUserRepository _userRepository;
    private readonly IValidationService _validationService;
    private readonly IToastService _toastService;

    public ProfileFacade(
        IAuthRepository authRepository,
        IUserRepository userRepository,
        IValidationService validationService,
        IToastService toastService)
    {
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    }

    /// <inheritdoc/>
    public async Task<Result<User>> LoadProfileAsync()
    {
        try
        {
            var result = await _authRepository.GetCurrentUserAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                return Result<User>.Failure(result.ErrorMessage ?? "Failed to load profile");
            }

            return Result<User>.Success(result.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] LoadProfileAsync failed: {ex.Message}");
            return Result<User>.Failure("Failed to load profile", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<User>> UpdateProfileAsync(string fullName, string email, string phoneNumber, string address)
    {
        try
        {
            // Step 1: Validate inputs
            var emailValidation = await _validationService.ValidateEmail(email);
            if (!emailValidation.IsSuccess || emailValidation.Data == null || !emailValidation.Data.IsValid)
            {
                var error = emailValidation.Data?.ErrorMessage ?? "Invalid email";
                return Result<User>.Failure(error);
            }

            var phoneValidation = await _validationService.ValidatePhoneNumber(phoneNumber);
            if (!phoneValidation.IsSuccess || phoneValidation.Data == null || !phoneValidation.Data.IsValid)
            {
                var error = phoneValidation.Data?.ErrorMessage ?? "Invalid phone number";
                return Result<User>.Failure(error);
            }

            // Step 2: Get current user ID
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess)
            {
                return Result<User>.Failure(userIdResult.ErrorMessage ?? "Failed to get user ID");
            }

            // Step 3: Update profile using DTO
            var request = new MyShop.Shared.DTOs.Requests.UpdateProfileRequest
            {
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Address = address
            };
            
            var updateResult = await _userRepository.UpdateProfileAsync(request);

            if (!updateResult.IsSuccess || updateResult.Data == null)
            {
                return Result<User>.Failure(updateResult.ErrorMessage ?? "Failed to update profile");
            }

            // Step 4: Show success notification
            await _toastService.ShowSuccess("Profile updated successfully!");

            return Result<User>.Success(updateResult.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] UpdateProfileAsync failed: {ex.Message}");
            return Result<User>.Failure("Failed to update profile", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword)
    {
        try
        {
            // Step 1: Validate passwords
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                return Result<Unit>.Failure("Current password is required");
            }

            var passwordValidation = await _validationService.ValidatePassword(newPassword);
            if (!passwordValidation.IsSuccess || passwordValidation.Data == null || !passwordValidation.Data.IsValid)
            {
                var error = passwordValidation.Data?.ErrorMessage ?? "Invalid new password";
                return Result<Unit>.Failure(error);
            }

            if (newPassword != confirmPassword)
            {
                return Result<Unit>.Failure("New password and confirmation password do not match");
            }

            if (currentPassword == newPassword)
            {
                return Result<Unit>.Failure("New password must be different from current password");
            }

            // Step 2: Get current user ID
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess)
            {
                return Result<Unit>.Failure(userIdResult.ErrorMessage ?? "Failed to get user ID");
            }

            // Step 3: Change password using DTO
            var request = new MyShop.Shared.DTOs.Requests.ChangePasswordRequest
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };
            
            var changeResult = await _userRepository.ChangePasswordAsync(request);

            if (!changeResult.IsSuccess || !changeResult.Data)
            {
                return Result<Unit>.Failure(changeResult.ErrorMessage ?? "Failed to change password");
            }

            // Step 4: Show success notification
            await _toastService.ShowSuccess("Password changed successfully!");

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] ChangePasswordAsync failed: {ex.Message}");
            return Result<Unit>.Failure("Failed to change password", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<string>> PickAndSaveAvatarAsync()
    {
        try
        {
            // Step 1: Open file picker
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            // Get the window handle for WinUI 3
            var window = App.MainWindow;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return Result<string>.Failure("No file selected");
            }

            // Step 2: Save to local storage
            var saveResult = await SaveAvatarLocallyAsync(file);
            if (!saveResult.IsSuccess)
            {
                return Result<string>.Failure(saveResult.ErrorMessage ?? "Failed to save avatar");
            }

            return Result<string>.Success(saveResult.Data!);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] PickAndSaveAvatarAsync failed: {ex.Message}");
            return Result<string>.Failure("Failed to pick avatar", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<string>> SaveAvatarLocallyAsync(StorageFile file)
    {
        try
        {
            // Step 1: Get user ID and avatar directory using StorageConstants
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            var userId = userIdResult.IsSuccess ? userIdResult.Data.ToString() : Guid.NewGuid().ToString();
            
            // Use StorageConstants for consistent path management (works in unpackaged WinUI 3)
            var avatarDirectory = StorageConstants.GetUserAvatarDirectory(userId!);
            StorageConstants.EnsureDirectoryExists(avatarDirectory);

            // Step 2: Generate unique filename
            var extension = Path.GetExtension(file.Name);
            var fileName = $"{userId}_avatar{extension}";
            var targetPath = Path.Combine(avatarDirectory, fileName);

            // Step 3: Copy file to local storage using standard .NET file I/O
            using var sourceStream = await file.OpenStreamForReadAsync();
            using var targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
            await sourceStream.CopyToAsync(targetStream);

            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] Avatar saved to: {targetPath}");

            await _toastService.ShowSuccess("Avatar updated successfully!");

            return Result<string>.Success(targetPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] SaveAvatarLocallyAsync failed: {ex.Message}");
            return Result<string>.Failure("Failed to save avatar locally", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<string>> SaveAvatarLocallyAsync(string fileName, Stream fileStream)
    {
        try
        {
            // Validate stream
            if (fileStream == null)
            {
                return Result<string>.Failure("File stream is null");
            }

            if (!fileStream.CanRead)
            {
                return Result<string>.Failure("File stream is not readable");
            }

            // Step 1: Get user ID and avatar directory using StorageConstants
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            var userId = userIdResult.IsSuccess ? userIdResult.Data.ToString() : Guid.NewGuid().ToString();
            
            // Use StorageConstants for consistent path management (works in unpackaged WinUI 3)
            var avatarDirectory = StorageConstants.GetUserAvatarDirectory(userId!);
            StorageConstants.EnsureDirectoryExists(avatarDirectory);

            // Step 2: Generate unique filename
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{userId}_avatar{extension}";
            var targetPath = Path.Combine(avatarDirectory, uniqueFileName);

            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] Saving avatar to: {targetPath}");

            // Step 3: Reset stream position if seekable
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            // Step 4: Save stream to file using standard .NET file I/O
            using (var targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(targetStream);
            }

            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] Avatar saved from stream to: {targetPath}");

            await _toastService.ShowSuccess("Avatar updated successfully!");

            return Result<string>.Success(targetPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] SaveAvatarLocallyAsync(stream) failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] Stack trace: {ex.StackTrace}");
            return Result<string>.Failure("Failed to save avatar from stream", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> SendVerificationEmailAsync()
    {
        try
        {
            // Get current user ID
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess)
            {
                return Result<Unit>.Failure(userIdResult.ErrorMessage ?? "Failed to get user ID");
            }

            // Send verification email
            var sendResult = await _authRepository.SendVerificationEmailAsync(userIdResult.Data.ToString());
            if (!sendResult.IsSuccess)
            {
                return Result<Unit>.Failure(sendResult.ErrorMessage ?? "Failed to send verification email");
            }

            await _toastService.ShowSuccess("Verification email sent! Please check your inbox.");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] SendVerificationEmailAsync failed: {ex.Message}");
            return Result<Unit>.Failure("Failed to send verification email", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> VerifyEmailAsync(string otp)
    {
        try
        {
            // Validate OTP
            if (string.IsNullOrWhiteSpace(otp) || otp.Length != 6)
            {
                return Result<Unit>.Failure("Please enter a valid 6-digit OTP code");
            }

            // Get current user ID
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess)
            {
                return Result<Unit>.Failure(userIdResult.ErrorMessage ?? "Failed to get user ID");
            }

            // Verify email
            var verifyResult = await _authRepository.VerifyEmailAsync(userIdResult.Data.ToString(), otp);
            if (!verifyResult.IsSuccess)
            {
                return Result<Unit>.Failure(verifyResult.ErrorMessage ?? "Email verification failed");
            }

            await _toastService.ShowSuccess("Email verified successfully!");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] VerifyEmailAsync failed: {ex.Message}");
            return Result<Unit>.Failure("Failed to verify email", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> UpdateTaxRateAsync(decimal taxRate)
    {
        try
        {
            // Validate tax rate
            if (taxRate < 0 || taxRate > 100)
            {
                return Result<Unit>.Failure("Tax rate must be between 0% and 100%");
            }

            // Tax rate update is not directly supported by IUserRepository
            // This would need to be implemented via a settings or configuration service
            // For now, just store it locally or show a message
            await _toastService.ShowInfo($"Tax rate setting: {taxRate}% (feature pending implementation)");
            
            // TODO: Implement via ISettingsService or extend IUserRepository
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProfileFacade] UpdateTaxRateAsync failed: {ex.Message}");
            return Result<Unit>.Failure("Failed to update tax rate", ex);
        }
    }
}
