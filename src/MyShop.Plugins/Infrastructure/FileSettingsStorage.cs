using MyShop.Core.Common;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Shared.Models;
using System.Text.Json;

namespace MyShop.Plugins.Storage;

/// <summary>
/// File-based implementation of ISettingsStorage
/// Stores settings as JSON in LocalApplicationData folder
/// </summary>
public class FileSettingsStorage : ISettingsStorage
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public FileSettingsStorage()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(localAppData, "MyShop2025");
        
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        _filePath = Path.Combine(appFolder, "settings.json");
        System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Settings file: {_filePath}");
    }

    public async Task<Result<AppSettings>> GetAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                System.Diagnostics.Debug.WriteLine("[FileSettingsStorage] Settings file not found, using defaults");
                return Result<AppSettings>.Success(new AppSettings());
            }

            await using var fs = File.OpenRead(_filePath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(fs, _jsonOptions);
            
            System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Loaded settings: Theme={settings?.Theme}, Language={settings?.Language}");
            
            return Result<AppSettings>.Success(settings ?? new AppSettings());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Error loading settings: {ex.Message}");
            return Result<AppSettings>.Failure($"Failed to load settings: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> SaveAsync(AppSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
            
            System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Settings saved: Theme={settings.Theme}, Language={settings.Language}");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Error saving settings: {ex.Message}");
            return Result<Unit>.Failure($"Failed to save settings: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> ResetAsync()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
                System.Diagnostics.Debug.WriteLine("[FileSettingsStorage] Settings file deleted");
            }
            
            await Task.CompletedTask;
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Error resetting settings: {ex.Message}");
            return Result<Unit>.Failure($"Failed to reset settings: {ex.Message}");
        }
    }
}
