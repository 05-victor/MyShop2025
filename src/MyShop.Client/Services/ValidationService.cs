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
    public async Task<Result<ValidationResult>> ValidateUsername(string username)
    {
        await Task.CompletedTask;
        
        if (string.IsNullOrWhiteSpace(username))
            return Result<ValidationResult>.Success(ValidationResult.Failure("Username or Email is required"));

        if (username.Length < 3)
            return Result<ValidationResult>.Success(ValidationResult.Failure("Username must be at least 3 characters"));

        if (username.Length > 50)
            return Result<ValidationResult>.Success(ValidationResult.Failure("Username must not exceed 50 characters"));

        return Result<ValidationResult>.Success(ValidationResult.Success());
    }

    /// <inheritdoc/>
    public async Task<Result<ValidationResult>> ValidatePassword(string password)
    {
        await Task.CompletedTask;
        
        if (string.IsNullOrWhiteSpace(password))
            return Result<ValidationResult>.Success(ValidationResult.Failure("Password is required"));

        if (password.Length < 6)
            return Result<ValidationResult>.Success(ValidationResult.Failure("Password must be at least 6 characters"));

        if (password.Length > 100)
            return Result<ValidationResult>.Success(ValidationResult.Failure("Password must not exceed 100 characters"));

        // Optional: Thêm quy tắc phức tạp hơn nếu cần
        // - Phải có chữ hoa
        // - Phải có chữ thường
        // - Phải có số
        // - Phải có ký tự đặc biệt

        return Result<ValidationResult>.Success(ValidationResult.Success());
    }

    /// <inheritdoc/>
    public async Task<Result<ValidationResult>> ValidateEmail(string email)
    {
        await Task.CompletedTask;
        
        if (string.IsNullOrWhiteSpace(email))
            return Result<ValidationResult>.Success(ValidationResult.Failure("Email is required"));

        if (!EmailRegex.IsMatch(email))
            return Result<ValidationResult>.Success(ValidationResult.Failure("Invalid email format"));

        if (email.Length > 100)
            return Result<ValidationResult>.Success(ValidationResult.Failure("Email must not exceed 100 characters"));

        return Result<ValidationResult>.Success(ValidationResult.Success());
    }

    /// <inheritdoc/>
    public async Task<Result<ValidationResult>> ValidatePasswordConfirmation(string password, string confirmPassword)
    {
        await Task.CompletedTask;
        
        if (string.IsNullOrWhiteSpace(confirmPassword))
            return Result<ValidationResult>.Success(ValidationResult.Failure("Password confirmation is required"));

        if (password != confirmPassword)
            return Result<ValidationResult>.Success(ValidationResult.Failure("Passwords do not match"));

        return Result<ValidationResult>.Success(ValidationResult.Success());
    }

    /// <inheritdoc/>
    public async Task<Result<ValidationResult>> ValidatePhoneNumber(string phoneNumber)
    {
        await Task.CompletedTask;
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result<ValidationResult>.Success(ValidationResult.Failure("Phone number is required"));

        // Accept phone numbers with 10-20 digits, may contain spaces, dashes, or parentheses
        var phonePattern = @"^[\d\s\-\(\)]{10,20}$";
        if (!Regex.IsMatch(phoneNumber, phonePattern))
            return Result<ValidationResult>.Success(ValidationResult.Failure("Invalid phone number format"));

        return Result<ValidationResult>.Success(ValidationResult.Success());
    }

    /// <inheritdoc/>
    public async Task<Result<ValidationResult>> ValidateRequired(string value, string fieldName)
    {
        await Task.CompletedTask;
        
        if (string.IsNullOrWhiteSpace(value))
            return Result<ValidationResult>.Success(ValidationResult.Failure($"{fieldName} is required"));

        return Result<ValidationResult>.Success(ValidationResult.Success());
    }
}
