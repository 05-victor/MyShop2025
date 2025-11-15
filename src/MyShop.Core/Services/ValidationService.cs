using MyShop.Core.Interfaces.Services;
using System.Text.RegularExpressions;

namespace MyShop.Core.Services;

/// <summary>
/// Implementation của IValidationService với các quy tắc validation chuẩn
/// </summary>
public class ValidationService : IValidationService
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <inheritdoc/>
    public ValidationResult ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return ValidationResult.Failure("Username or Email is required");

        if (username.Length < 3)
            return ValidationResult.Failure("Username must be at least 3 characters");

        if (username.Length > 50)
            return ValidationResult.Failure("Username must not exceed 50 characters");

        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    public ValidationResult ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return ValidationResult.Failure("Password is required");

        if (password.Length < 6)
            return ValidationResult.Failure("Password must be at least 6 characters");

        if (password.Length > 100)
            return ValidationResult.Failure("Password must not exceed 100 characters");

        // Optional: Thêm quy tắc phức tạp hơn nếu cần
        // - Phải có chữ hoa
        // - Phải có chữ thường
        // - Phải có số
        // - Phải có ký tự đặc biệt

        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    public ValidationResult ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return ValidationResult.Failure("Email is required");

        if (!EmailRegex.IsMatch(email))
            return ValidationResult.Failure("Invalid email format");

        if (email.Length > 100)
            return ValidationResult.Failure("Email must not exceed 100 characters");

        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    public ValidationResult ValidatePasswordConfirmation(string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(confirmPassword))
            return ValidationResult.Failure("Password confirmation is required");

        if (password != confirmPassword)
            return ValidationResult.Failure("Passwords do not match");

        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    public ValidationResult ValidatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return ValidationResult.Failure("Phone number is required");

        // Accept phone numbers with 10-20 digits, may contain spaces, dashes, or parentheses
        var phonePattern = @"^[\d\s\-\(\)]{10,20}$";
        if (!Regex.IsMatch(phoneNumber, phonePattern))
            return ValidationResult.Failure("Invalid phone number format");

        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    public ValidationResult ValidateRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure($"{fieldName} is required");

        return ValidationResult.Success();
    }
}
