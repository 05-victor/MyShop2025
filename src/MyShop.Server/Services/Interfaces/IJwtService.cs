using MyShop.Data.Entities;
using System.Security.Claims;

namespace MyShop.Server.Services.Interfaces
{
    /// <summary>
    /// Interface for JWT token generation and validation services
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generate a JWT access token for the specified user
        /// </summary>
        /// <param name="user">User entity to generate token for</param>
        /// <returns>JWT token string</returns>
        Task<string> GenerateAccessTokenAsync(User user);

        /// <summary>
        /// Generate a refresh token
        /// </summary>
        /// <param name="ipAddress">IP address of the client requesting the token</param>
        /// <returns>Refresh token string</returns>
        string GenerateRefreshToken(string? ipAddress = null);

        /// <summary>
        /// Validate a JWT token and extract claims
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>ClaimsPrincipal if valid, null if invalid</returns>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Get access token expiration time from configuration
        /// </summary>
        /// <returns>Access token lifetime in minutes</returns>
        int GetAccessTokenExpirationMinutes();

        /// <summary>
        /// Get refresh token expiration time from configuration
        /// </summary>
        /// <returns>Refresh token lifetime in days</returns>
        int GetRefreshTokenExpirationDays();
    }
}