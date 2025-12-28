using MyShop.Core.Common;
using MyShop.Core.Interfaces.Infrastructure;
using System;
using System.IO;
using System.Text.Json;

namespace MyShop.Plugins.Infrastructure;

/// <summary>
/// File-based credential storage for development/testing.
/// Saves token to a JSON file - simple but insecure, use only in dev mode.
/// For production, use SecureCredentialStorage with DPAPI encryption.
/// </summary>
public class FileCredentialStorage : ICredentialStorage
{
    private readonly string _filePath;
    private string? _sessionAccessToken;
    private string? _sessionRefreshToken;

    public FileCredentialStorage()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "MyShop2025");
        Directory.CreateDirectory(appFolder);
        _filePath = Path.Combine(appFolder, "credentials.json");
    }

    public async Task<Result<Unit>> SaveToken(string accessToken, string? refreshToken = null, bool persistToFile = true)
    {
        try
        {
            // Always save to session (memory)
            _sessionAccessToken = accessToken;
            _sessionRefreshToken = refreshToken;
            System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] Session tokens saved to memory");

            // Only save to file if persistToFile=true
            if (!persistToFile)
            {
                System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] Skipping file storage (persistToFile=false)");
                return Result<Unit>.Success(Unit.Value);
            }

            var data = new TokenData
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                SavedAt = DateTime.UtcNow
            };
            var json = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(_filePath, json);
            System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] Tokens saved to file");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] SaveToken failed: {ex.Message}");
            return Result<Unit>.Failure($"Failed to save tokens: {ex.Message}");
        }
    }

    public string? GetToken()
    {
        try
        {
            // Check session first
            if (!string.IsNullOrEmpty(_sessionAccessToken))
            {
                System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] Returning session access token from memory");
                return _sessionAccessToken;
            }

            if (!File.Exists(_filePath))
            {
                // Return session token if available (for rememberMe=false case)
                if (!string.IsNullOrEmpty(_sessionAccessToken))
                {
                    System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] Returning session token (no persistent file)");
                    return _sessionAccessToken;
                }
                return null;
            }

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<TokenData>(json);
            if (data?.Token != null)
            {
                _sessionAccessToken = data.Token;
                _sessionRefreshToken = data.RefreshToken;
                System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] Loaded tokens from file to session (AccessToken: yes, RefreshToken: {(!string.IsNullOrEmpty(_sessionRefreshToken) ? "yes" : "no")})");
            }
            return data?.Token;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] GetToken failed: {ex.Message}");
            return null;
        }
    }

    public string? GetRefreshToken()
    {
        try
        {
            // Check session first
            if (!string.IsNullOrEmpty(_sessionRefreshToken))
            {
                System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] Returning session refresh token from memory");
                return _sessionRefreshToken;
            }

            if (!File.Exists(_filePath))
            {
                // Return session token if available (for rememberMe=false case)
                if (!string.IsNullOrEmpty(_sessionRefreshToken))
                {
                    System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] Returning session refresh token (no persistent file)");
                    return _sessionRefreshToken;
                }
                return null;
            }

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<TokenData>(json);
            if (data?.RefreshToken != null)
            {
                _sessionRefreshToken = data.RefreshToken;
                _sessionAccessToken = data.Token;
                System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] Loaded tokens from file to session (AccessToken: {(!string.IsNullOrEmpty(_sessionAccessToken) ? "yes" : "no")}, RefreshToken: yes)");
            }
            return data?.RefreshToken;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] GetRefreshToken failed: {ex.Message}");
            return null;
        }
    }

    public async Task<Result<Unit>> RemoveToken()
    {
        try
        {
            // Clear session tokens
            _sessionAccessToken = null;
            _sessionRefreshToken = null;
            System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] Session tokens cleared");

            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] RemoveToken failed: {ex.Message}");
            return Result<Unit>.Failure($"Failed to remove token: {ex.Message}");
        }
    }

    private class TokenData
    {
        public string Token { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime SavedAt { get; set; }
    }
}
