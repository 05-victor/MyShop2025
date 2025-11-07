using MyShop.Shared.DTOs;

namespace MyShop.Server.Services.Interfaces
{
    /// <summary>
    /// Interface for email verification service that handles generating verification tokens
    /// and verifying user emails through secure token-based URLs.
    /// </summary>
    public interface IEmailVerificationService
    {
        /// <summary>
        /// Generates a secure verification token for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>A secure JWT token containing user ID and expiration</returns>
        string GenerateVerificationToken(Guid userId);

        /// <summary>
        /// Sends a verification email to the user with a secure verification URL.
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ServiceResult indicating success or failure</returns>
        Task<ServiceResult> SendVerificationEmailAsync( CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies a user's email using the provided verification token.
        /// Updates the IsEmailVerified field in the database.
        /// </summary>
        /// <param name="token">The verification token from the URL</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ServiceResult indicating success or failure</returns>
        Task<ServiceResult> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates the verification token and extracts the user ID.
        /// </summary>
        /// <param name="token">The verification token to validate</param>
        /// <returns>The user ID if token is valid, null otherwise</returns>
        Guid? ValidateVerificationToken(string token);
    }
}
