using MyShop.Core.Common;
using MyShop.Core.Interfaces.Services;
using System.Text.RegularExpressions;

namespace MyShop.Client.Services;

/// <summary>
/// Implementation của IValidationService với các quy tắc validation chuẩn
/// </summary>
public class ValidationService : IValidationService
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <inheritdoc/>
    public Task<Result<ValidationResult>> ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Username or Email is required")));

        if (username.Length < 3)
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Username must be at least 3 characters")));

        if (username.Length > 50)
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Username must not exceed 50 characters")));

        return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Success()));
    }

    /// <inheritdoc/>
    public Task<Result<ValidationResult>> ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Password is required")));

        if (password.Length < 6)
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Password must be at least 6 characters")));

        if (password.Length > 100)
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Password must not exceed 100 characters")));

        // Optional: Add complex rules if needed
        // - Must have uppercase
        // - Must have lowercase
        // - Must have number
        // - Must have special character

        return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Success()));
    }

    /// <inheritdoc/>
    public Task<Result<ValidationResult>> ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Email is required")));

        if (!EmailRegex.IsMatch(email))
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Invalid email format")));

        if (email.Length > 100)
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Email must not exceed 100 characters")));

        return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Success()));
    }

    /// <inheritdoc/>
    public Task<Result<ValidationResult>> ValidatePasswordConfirmation(string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(confirmPassword))
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Password confirmation is required")));

        if (password != confirmPassword)
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Passwords do not match")));

        return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Success()));
    }

    /// <inheritdoc/>
    public Task<Result<ValidationResult>> ValidatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Phone number is required")));

        // Accept phone numbers with 10-20 digits, may contain spaces, dashes, or parentheses
        var phonePattern = @"^[\d\s\-\(\)]{10,20}$";
        if (!Regex.IsMatch(phoneNumber, phonePattern))
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure("Invalid phone number format")));

        return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Success()));
    }

    /// <inheritdoc/>
    public Task<Result<ValidationResult>> ValidateRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Failure($"{fieldName} is required")));

        return Task.FromResult(Result<ValidationResult>.Success(ValidationResult.Success()));
    }
}
