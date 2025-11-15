namespace MyShop.Core.Interfaces.Storage;

/// <summary>
/// Interface for storing and retrieving authentication credentials (JWT tokens)
/// Cho phép swap giữa các implementation: Windows PasswordVault, File, Memory, etc.
/// </summary>
public interface ICredentialStorage
{
    /// <summary>
    /// Save authentication token to storage
    /// </summary>
    void SaveToken(string token);

    /// <summary>
    /// Retrieve authentication token from storage
    /// </summary>
    /// <returns>Token if exists, null otherwise</returns>
    string? GetToken();

    /// <summary>
    /// Remove authentication token from storage (logout)
    /// </summary>
    void RemoveToken();
}
