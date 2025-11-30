namespace MyShop.Core.Interfaces.Services;
using MyShop.Core.Common;

/// <summary>
/// Service providing validation logic for form inputs.
/// Validates common input types like username, password, email, and phone number.
/// Returns ValidationResult with status and error message.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validate username or email format.
    /// </summary>
    Task<Result<ValidationResult>> ValidateUsername(string username);

    /// <summary>
    /// Validate password strength and requirements.
    /// </summary>
    Task<Result<ValidationResult>> ValidatePassword(string password);

    /// <summary>
    /// Validate email format.
    /// </summary>
    Task<Result<ValidationResult>> ValidateEmail(string email);

    /// <summary>
    /// Validate password confirmation (must match original password).
    /// </summary>
    Task<Result<ValidationResult>> ValidatePasswordConfirmation(string password, string confirmPassword);

    /// <summary>
    /// Validate phone number format.
    /// </summary>
    Task<Result<ValidationResult>> ValidatePhoneNumber(string phoneNumber);

    /// <summary>
    /// Validate required field with custom field name.
    /// </summary>
    Task<Result<ValidationResult>> ValidateRequired(string value, string fieldName);
}

/// <summary>
/// Validation result containing status and error message.
/// Immutable record-like class for validation outcomes.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Validation status (true = valid, false = invalid).
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Error message (empty if valid).
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// Create a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Create a failed validation result with error message.
    /// </summary>
    public static ValidationResult Failure(string message) => new() { IsValid = false, ErrorMessage = message };
}
