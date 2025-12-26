using MyShop.Core.Common;

namespace MyShop.Core.Interfaces.Infrastructure;

/// <summary>
/// Interface for storing and retrieving authentication credentials (JWT tokens).
/// Allows swapping between implementations: Windows PasswordVault, File, Memory, etc.
/// Implementations should handle secure storage with encryption (e.g., DPAPI on Windows).
/// </summary>
public interface ICredentialStorage
{
    /// <summary>
    /// Save authentication tokens to storage
    /// </summary>
    /// <param name="accessToken">JWT access token</param>
    /// <param name="refreshToken">Optional refresh token</param>
    Task<Result<Unit>> SaveToken(string accessToken, string? refreshToken = null);

    /// <summary>
    /// Retrieve access token from storage
    /// </summary>
    /// <returns>Access token if exists, null otherwise</returns>
    string? GetToken();

    /// <summary>
    /// Retrieve refresh token from storage
    /// </summary>
    /// <returns>Refresh token if exists, null otherwise</returns>
    string? GetRefreshToken();

    /// <summary>
    /// Remove authentication tokens from storage (logout)
    /// </summary>
    Task<Result<Unit>> RemoveToken();
}
