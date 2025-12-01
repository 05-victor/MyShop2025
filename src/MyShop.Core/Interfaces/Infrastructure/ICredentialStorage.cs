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
    /// Save authentication token to storage
    /// </summary>
    Task<Result<Unit>> SaveToken(string token);

    /// <summary>
    /// Retrieve authentication token from storage
    /// </summary>
    /// <returns>Token if exist s, null otherwise</returns>
    string? GetToken();

    /// <summary>
    /// Remove authentication token from storage (logout)
    /// </summary>
    Task<Result<Unit>> RemoveToken();
}
