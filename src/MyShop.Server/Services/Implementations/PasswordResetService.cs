using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs;

namespace MyShop.Server.Services.Implementations;

/// <summary>
/// Service for handling password reset functionality.
/// Uses in-memory cache to store temporary reset codes with expiration.
/// Integrates with EmailNotificationService for sending reset codes.
/// </summary>
public class PasswordResetService : IPasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailNotificationService _emailService;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PasswordResetService> _logger;
    private readonly int _resetCodeExpirationMinutes;
    private readonly int _resetCodeLength;

    public PasswordResetService(
        IUserRepository userRepository,
        IEmailNotificationService emailService,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<PasswordResetService> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;

        // Get configuration values with defaults
        _resetCodeExpirationMinutes = _configuration.GetValue<int>("PasswordResetSettings:ResetCodeExpirationMinutes", 15);
        _resetCodeLength = _configuration.GetValue<int>("PasswordResetSettings:ResetCodeLength", 6);
    }

    /// <summary>
    /// Generates a random numeric code for password reset.
    /// </summary>
    private string GenerateResetCode()
    {
        var random = new Random();
        var code = new char[_resetCodeLength];
        for (int i = 0; i < _resetCodeLength; i++)
        {
            code[i] = (char)('0' + random.Next(0, 10));
        }
        return new string(code);
    }

    /// <summary>
    /// Sends a password reset code to the user's email.
    /// </summary>
    public async Task<ServiceResult> SendPasswordResetCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate email
            if (string.IsNullOrWhiteSpace(email))
            {
                return ServiceResult.Failure("Email is required");
            }

            // Check if user exists
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                // For security, don't reveal whether email exists
                // Still return success to prevent email enumeration
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                return ServiceResult.Success("If the email exists, a reset code has been sent");
            }

            // Generate reset code
            var resetCode = GenerateResetCode();
            var cacheKey = $"password_reset_{email.ToLower()}";

            // Store code in cache with expiration
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_resetCodeExpirationMinutes));

            _cache.Set(cacheKey, resetCode, cacheOptions);

            // Prepare email placeholders
            var placeholders = new Dictionary<string, string>
            {
                { "USERNAME", user.Username },
                { "RESET_CODE", resetCode },
                { "EXPIRATION_MINUTES", _resetCodeExpirationMinutes.ToString() }
            };

            // Send email using template
            var emailSent = await _emailService.SendEmailAsync(
                user.Email,
                user.Username,
                "Password Reset Request",
                "password-reset.html",
                placeholders);

            if (!emailSent)
            {
                _logger.LogError("Failed to send password reset email to {Email}", email);
                return ServiceResult.Failure("Failed to send password reset email");
            }

            _logger.LogInformation("Password reset code sent to {Email}", email);
            return ServiceResult.Success("If the email exists, a reset code has been sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset code for {Email}", email);
            return ServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates a reset code for a given email.
    /// </summary>
    public async Task<bool> ValidateResetCodeAsync(string email, string resetCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(resetCode))
            {
                return false;
            }

            var cacheKey = $"password_reset_{email.ToLower()}";

            // Check if code exists in cache
            if (!_cache.TryGetValue(cacheKey, out string? cachedCode))
            {
                _logger.LogWarning("Reset code not found or expired for email: {Email}", email);
                return false;
            }

            // Validate code matches
            if (cachedCode != resetCode)
            {
                _logger.LogWarning("Invalid reset code provided for email: {Email}", email);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating reset code for {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Resets password using a valid reset code.
    /// </summary>
    public async Task<ServiceResult> ResetPasswordAsync(string email, string resetCode, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(resetCode) || string.IsNullOrWhiteSpace(newPassword))
            {
                return ServiceResult.Failure("All fields are required");
            }

            // Validate reset code
            if (!await ValidateResetCodeAsync(email, resetCode))
            {
                return ServiceResult.Failure("Invalid or expired reset code");
            }

            // Get user
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return ServiceResult.Failure("User not found");
            }

            // Hash new password using BCrypt
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Update user in database
            await _userRepository.UpdateAsync(user);

            // Remove reset code from cache
            var cacheKey = $"password_reset_{email.ToLower()}";
            _cache.Remove(cacheKey);

            _logger.LogInformation("Password reset successfully for user {Username} (Email: {Email})", user.Username, email);
            return ServiceResult.Success("Password reset successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for {Email}", email);
            return ServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }
}
