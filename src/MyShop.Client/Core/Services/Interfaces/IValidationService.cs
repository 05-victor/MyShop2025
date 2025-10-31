using System;

namespace MyShop.Client.Core.Services.Interfaces
{
    /// <summary>
    /// Service cung cấp validation logic cho các form inputs
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validate username hoặc email
        /// </summary>
        /// <param name="username">Username cần validate</param>
        /// <returns>ValidationResult với trạng thái và thông báo lỗi (nếu có)</returns>
        ValidationResult ValidateUsername(string username);

        /// <summary>
        /// Validate password
        /// </summary>
        /// <param name="password">Password cần validate</param>
        /// <returns>ValidationResult với trạng thái và thông báo lỗi (nếu có)</returns>
        ValidationResult ValidatePassword(string password);

        /// <summary>
        /// Validate email format
        /// </summary>
        /// <param name="email">Email cần validate</param>
        /// <returns>ValidationResult với trạng thái và thông báo lỗi (nếu có)</returns>
        ValidationResult ValidateEmail(string email);

        /// <summary>
        /// Validate password confirmation (phải khớp với password gốc)
        /// </summary>
        /// <param name="password">Password gốc</param>
        /// <param name="confirmPassword">Password xác nhận</param>
        /// <returns>ValidationResult với trạng thái và thông báo lỗi (nếu có)</returns>
        ValidationResult ValidatePasswordConfirmation(string password, string confirmPassword);
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
        /// <param name="message">Thông báo lỗi</param>
        public static ValidationResult Failure(string message) => new() { IsValid = false, ErrorMessage = message };
    }
}
