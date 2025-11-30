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

    public FileCredentialStorage()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "MyShop2025");
        Directory.CreateDirectory(appFolder);
        _filePath = Path.Combine(appFolder, "credentials.json");
    }

    public async Task<Result<Unit>> SaveToken(string token)
    {
        try
        {
            var data = new { Token = token, SavedAt = DateTime.UtcNow };
            var json = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(_filePath, json);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] SaveToken failed: {ex.Message}");
            return Result<Unit>.Failure($"Failed to save token: {ex.Message}");
        }
    }

    public string? GetToken()
    {
        try
        {
            if (!File.Exists(_filePath))
                return null;

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<TokenData>(json);
            return data?.Token;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] GetToken failed: {ex.Message}");
            return null;
        }
    }

    public async Task<Result<Unit>> RemoveToken()
    {
        try
        {
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
        public DateTime SavedAt { get; set; }
    }
}
