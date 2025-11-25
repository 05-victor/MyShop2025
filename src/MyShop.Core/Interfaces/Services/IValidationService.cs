namespace MyShop.Core.Interfaces.Services;
using MyShop.Core.Common;

/// <summary>
/// Service cung cấp validation logic cho các form inputs
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validate username hoặc email
    /// </summary>
    Task<Result<ValidationResult>> ValidateUsername(string username);

    /// <summary>
    /// Validate password
    /// </summary>
    Task<Result<ValidationResult>> ValidatePassword(string password);

    /// <summary>
    /// Validate email format
    /// </summary>
    Task<Result<ValidationResult>> ValidateEmail(string email);

    /// <summary>
    /// Validate password confirmation (phải khớp với password gốc)
    /// </summary>
    Task<Result<ValidationResult>> ValidatePasswordConfirmation(string password, string confirmPassword);

    /// <summary>
    /// Validate phone number format
    /// </summary>
    Task<Result<ValidationResult>> ValidatePhoneNumber(string phoneNumber);

    /// <summary>
    /// Validate required field with custom field name
    /// </summary>
    Task<Result<ValidationResult>> ValidateRequired(string value, string fieldName);
}

/// <summary>
/// Kết quả validation với trạng thái và thông báo lỗi
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Trạng thái validation (true = hợp lệ, false = không hợp lệ)
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Thông báo lỗi (rỗng nếu hợp lệ)
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// Tạo kết quả validation thành công
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Tạo kết quả validation thất bại với thông báo lỗi
    /// </summary>
    public static ValidationResult Failure(string message) => new() { IsValid = false, ErrorMessage = message };
}
