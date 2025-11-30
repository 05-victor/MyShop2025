using MyShop.Core.Common;
using MyShop.Core.Interfaces.Infrastructure;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyShop.Plugins.Infrastructure;

/// <summary>
/// Secure credential storage using Windows DPAPI (Data Protection API).
/// 
/// Features:
/// - Encrypts tokens using DPAPI before saving to file
/// - Per-user credential files: users/{UserId}/credentials.enc
/// - Automatic migration from temp credentials after login
/// - Falls back to user-scope encryption (works across sessions)
/// 
/// Security:
/// - Only the same Windows user on the same machine can decrypt
/// - File contents are unreadable without the encryption key
/// - More flexible than Windows PasswordVault, works in all app types
/// </summary>
public class SecureCredentialStorage : ICredentialStorage
{
    private string? _currentUserId;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false
    };

    /// <summary>
    /// Set the current user ID (call after login to enable per-user storage)
    /// </summary>
    public void SetCurrentUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return;

        var previousUserId = _currentUserId;
        _currentUserId = userId;

        System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] User set: {userId}");

        // Ensure user directories exist
        StorageConstants.EnsureUserDirectoriesExist(userId);

        // Migrate temp credentials if they exist
        if (previousUserId == null)
        {
            MigrateFromTempCredentials();
        }
    }

    /// <summary>
    /// Clear the current user (call on logout)
    /// </summary>
    public void ClearCurrentUser()
    {
        System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] User cleared: {_currentUserId}");
        _currentUserId = null;
    }

    /// <summary>
    /// Get the current user ID
    /// </summary>
    public string? CurrentUserId => _currentUserId;

    public async Task<Result<Unit>> SaveToken(string token)
    {
        try
        {
            var filePath = GetCredentialsFilePath();
            var directory = Path.GetDirectoryName(filePath);
            
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create credential data with metadata
            var data = new CredentialData
            {
                Token = token,
                SavedAt = DateTime.UtcNow,
                UserId = _currentUserId
            };

            // Serialize to JSON
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var plainBytes = Encoding.UTF8.GetBytes(json);

            // Encrypt using DPAPI (CurrentUser scope - works across sessions)
            var encryptedBytes = ProtectedData.Protect(
                plainBytes,
                GetEntropy(),
                DataProtectionScope.CurrentUser);

            // Write encrypted data to file
            await File.WriteAllBytesAsync(filePath, encryptedBytes);

            // Save as last logged-in user for Remember Me
            if (!string.IsNullOrEmpty(_currentUserId))
            {
                StorageConstants.SaveLastLoggedInUser(_currentUserId);
            }

            System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] Token saved (encrypted) to: {filePath}");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] SaveToken failed: {ex.Message}");
            return Result<Unit>.Failure($"Failed to save token: {ex.Message}");
        }
    }

    public string? GetToken()
    {
        try
        {
            // If current user is set, use their credentials
            if (!string.IsNullOrEmpty(_currentUserId))
            {
                var filePath = GetCredentialsFilePath();
                if (File.Exists(filePath))
                {
                    return DecryptTokenFromFile(filePath);
                }
                System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] No credentials for current user: {_currentUserId}");
                return null;
            }

            // No current user - try to find last logged-in user for Remember Me
            var lastUserId = StorageConstants.GetLastLoggedInUser();
            if (!string.IsNullOrEmpty(lastUserId))
            {
                var credentialsFile = StorageConstants.GetUserCredentialsFile(lastUserId);
                if (File.Exists(credentialsFile))
                {
                    var token = DecryptTokenFromFile(credentialsFile);
                    if (!string.IsNullOrEmpty(token))
                    {
                        // Set this user as current
                        _currentUserId = lastUserId;
                        StorageConstants.SetCurrentUser(lastUserId);
                        System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] Auto-login from last user: {lastUserId}");
                        return token;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] Last user has no credentials: {lastUserId}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[SecureCredentialStorage] No last logged-in user found");
            }

            // Check temp credentials as fallback
            if (File.Exists(StorageConstants.TempCredentialsFile))
            {
                return DecryptTokenFromFile(StorageConstants.TempCredentialsFile);
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] GetToken failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Decrypt and return token from a credentials file
    /// </summary>
    private string? DecryptTokenFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            // Read encrypted data
            var encryptedBytes = File.ReadAllBytes(filePath);

            // Decrypt using DPAPI
            var plainBytes = ProtectedData.Unprotect(
                encryptedBytes,
                GetEntropy(),
                DataProtectionScope.CurrentUser);

            // Deserialize JSON
            var json = Encoding.UTF8.GetString(plainBytes);
            var data = JsonSerializer.Deserialize<CredentialData>(json, _jsonOptions);

            System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] Token loaded (decrypted) from: {filePath}");
            return data?.Token;
        }
        catch (CryptographicException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] Decryption failed for {filePath}: {ex.Message}");
            // Try to delete corrupted file
            try { File.Delete(filePath); } catch { }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] DecryptTokenFromFile failed: {ex.Message}");
            return null;
        }
    }

    public async Task<Result<Unit>> RemoveToken()
    {
        try
        {
            var filePath = GetCredentialsFilePath();
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] Credentials deleted: {filePath}");
            }

            // Also clean up temp credentials if any
            if (File.Exists(StorageConstants.TempCredentialsFile))
            {
                File.Delete(StorageConstants.TempCredentialsFile);
            }

            // Clear last logged-in user on logout (user explicitly logged out)
            StorageConstants.ClearLastLoggedInUser();

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] RemoveToken failed: {ex.Message}");
            return Result<Unit>.Failure($"Failed to remove token: {ex.Message}");
        }
    }

    #region Private Methods

    /// <summary>
    /// Get the path to the credentials file
    /// Uses per-user path if user is set, otherwise temp path
    /// </summary>
    private string GetCredentialsFilePath()
    {
        if (!string.IsNullOrEmpty(_currentUserId))
        {
            return StorageConstants.GetUserCredentialsFile(_currentUserId);
        }

        // No user set - use temp location
        StorageConstants.EnsureDirectoryExists(StorageConstants.SessionDirectory);
        return StorageConstants.TempCredentialsFile;
    }

    /// <summary>
    /// Get additional entropy for DPAPI encryption
    /// This adds an extra layer of protection
    /// </summary>
    private static byte[] GetEntropy()
    {
        // Use a fixed entropy based on app name
        // This ensures only our app can decrypt
        return Encoding.UTF8.GetBytes($"{StorageConstants.AppName}_Entropy_v1");
    }

    /// <summary>
    /// Migrate credentials from temp location to user-specific location
    /// Called after user ID is set
    /// </summary>
    private void MigrateFromTempCredentials()
    {
        try
        {
            var tempFile = StorageConstants.TempCredentialsFile;
            
            if (!File.Exists(tempFile) || string.IsNullOrEmpty(_currentUserId))
                return;

            var userFile = StorageConstants.GetUserCredentialsFile(_currentUserId);
            var userDir = Path.GetDirectoryName(userFile);

            if (!string.IsNullOrEmpty(userDir))
            {
                Directory.CreateDirectory(userDir);
            }

            // Move the file (it's already encrypted, just relocate it)
            if (File.Exists(userFile))
            {
                File.Delete(userFile);
            }
            
            File.Move(tempFile, userFile);
            
            System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] Migrated temp credentials to: {userFile}");

            // Clean up session directory if empty
            StorageConstants.CleanupSessionDirectory();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] Migration failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Try to delete the credentials file (used on decryption failure)
    /// </summary>
    private void TryDeleteCredentialsFile()
    {
        try
        {
            var filePath = GetCredentialsFilePath();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                System.Diagnostics.Debug.WriteLine($"[SecureCredentialStorage] Deleted corrupted credentials file: {filePath}");
            }
        }
        catch
        {
            // Ignore deletion errors
        }
    }

    #endregion

    #region Credential Data Model

    private class CredentialData
    {
        public string Token { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
        public string? UserId { get; set; }
    }

    #endregion
}
