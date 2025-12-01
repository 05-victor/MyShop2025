using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Facades;

/// <summary>
/// Facade pattern for profile management operations
/// Aggregates: IAuthRepository, IUserRepository, IValidationService, IToastService
/// </summary>
public interface IProfileFacade
{
    /// <summary>
    /// Load current user profile
    /// </summary>
    Task<Result<User>> LoadProfileAsync();

    /// <summary>
    /// Update profile with validation.
    /// Orchestrates: Validation → Repository.UpdateProfile → Toast notification
    /// </summary>
    Task<Result<User>> UpdateProfileAsync(string fullName, string email, string phoneNumber, string address);

    /// <summary>
    /// Change password with validation.
    /// Orchestrates: Validation → Repository.ChangePassword → Toast notification
    /// </summary>
    Task<Result<Unit>> ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword);

    /// <summary>
    /// Pick avatar from file picker and save locally.
    /// Saves to ApplicationData.Current.LocalFolder/Assets/Avatars/
    /// Returns local file path.
    /// </summary>
    Task<Result<string>> PickAndSaveAvatarAsync();

    /// <summary>
    /// Upload avatar file to local storage from Stream
    /// Does NOT upload to server - stores in ApplicationData.LocalFolder
    /// </summary>
    /// <param name="fileName">File name with extension</param>
    /// <param name="fileStream">File content stream</param>
    Task<Result<string>> SaveAvatarLocallyAsync(string fileName, Stream fileStream);

    /// <summary>
    /// Send email verification OTP
    /// </summary>
    Task<Result<Unit>> SendVerificationEmailAsync();

    /// <summary>
    /// Verify email with OTP code.
    /// </summary>
    Task<Result<Unit>> VerifyEmailAsync(string otp);

    /// <summary>
    /// Update tax rate (for sales agents)
    /// </summary>
    Task<Result<Unit>> UpdateTaxRateAsync(decimal taxRate);
}
