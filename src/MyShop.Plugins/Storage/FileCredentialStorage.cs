using MyShop.Core.Interfaces.Storage;
using System;
using System.IO;
using System.Text.Json;

namespace MyShop.Plugins.Storage;

/// <summary>
/// File-based credential storage for development/testing
/// Lưu token vào file JSON - đơn giản nhưng kém bảo mật, chỉ dùng dev mode
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

    public void SaveToken(string token)
    {
        try
        {
            var data = new { Token = token, SavedAt = DateTime.UtcNow };
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] SaveToken failed: {ex.Message}");
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

    public void RemoveToken()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileCredentialStorage] RemoveToken failed: {ex.Message}");
        }
    }

    private class TokenData
    {
        public string Token { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
    }
}
