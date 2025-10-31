using MyShop.Client.Core.Services.Interfaces;
using System.Text.RegularExpressions;

namespace MyShop.Client.Core.Services.Implementations
{
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
    }
}
