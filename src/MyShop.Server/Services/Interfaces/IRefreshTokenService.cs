using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces
{
    /// <summary>
    /// Service for managing refresh tokens.
    /// Handles token generation, validation, rotation, and revocation.
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Create and save a new refresh token for a user.
        /// </summary>
        /// <param name="user">User to create token for</param>
        /// <param name="ipAddress">IP address of the client</param>
        /// <returns>Created refresh token entity</returns>
        Task<RefreshToken> CreateRefreshTokenAsync(User user, string? ipAddress = null);

        /// <summary>
        /// Refresh access token using a refresh token.
        /// Implements token rotation for security.
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <param name="ipAddress">IP address of the client</param>
        /// <returns>New access token and optionally new refresh token</returns>
        Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null);

        /// <summary>
        /// Revoke a refresh token.
        /// </summary>
        /// <param name="token">Token string to revoke</param>
        /// <param name="ipAddress">IP address of the client</param>
        /// <param name="reason">Reason for revocation</param>
        /// <returns>True if revoked successfully</returns>
        Task<bool> RevokeTokenAsync(string token, string? ipAddress = null, string? reason = null);

        /// <summary>
        /// Revoke all refresh tokens for a user (e.g., on logout or password change).
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="ipAddress">IP address of the client</param>
        /// <param name="reason">Reason for revocation</param>
        /// <returns>Number of tokens revoked</returns>
        Task<int> RevokeAllUserTokensAsync(Guid userId, string? ipAddress = null, string? reason = null);

        /// <summary>
        /// Clean up expired and revoked tokens (for maintenance).
        /// </summary>
        /// <returns>Number of tokens deleted</returns>
        Task<int> CleanupExpiredTokensAsync();
    }
}
