using Microsoft.EntityFrameworkCore;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;

namespace MyShop.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for managing application settings
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly ShopContext _context;

    public SettingsRepository(ShopContext context)
    {
        _context = context;
    }

    public async Task<AppSetting?> GetAppSettingsAsync()
    {
        // Get the first (and should be only) app settings record
        return await _context.AppSettings.FirstOrDefaultAsync();
    }

    public async Task<AppSetting> UpdateAppSettingsAsync(AppSetting appSetting)
    {
        appSetting.UpdatedAt = DateTime.UtcNow;
        _context.AppSettings.Update(appSetting);
        await _context.SaveChangesAsync();
        return appSetting;
    }

    public async Task<UserPreference?> GetUserPreferenceAsync(Guid userId)
    {
        return await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userId);
    }

    public async Task<UserPreference> UpsertUserPreferenceAsync(UserPreference userPreference)
    {
        userPreference.UpdatedAt = DateTime.UtcNow;
        
        var existing = await GetUserPreferenceAsync(userPreference.UserId);
        if (existing != null)
        {
            existing.Theme = userPreference.Theme;
            existing.UpdatedAt = userPreference.UpdatedAt;
            _context.UserPreferences.Update(existing);
        }
        else
        {
            _context.UserPreferences.Add(userPreference);
        }
        
        await _context.SaveChangesAsync();
        return existing ?? userPreference;
    }
}
