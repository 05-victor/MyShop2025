using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Windows.Storage;

namespace MyShop.Client.Services;

/// <summary>
/// Service for tracking navigation history, recent items, and favorites.
/// Persists data across sessions using local settings.
/// </summary>
public class NavigationHistoryService
{
    private const int MaxRecentItems = 10;
    private const string RecentItemsKey = "NavigationHistory_RecentItems";
    private const string FavoritesKey = "NavigationHistory_Favorites";

    private readonly ApplicationDataContainer _localSettings;
    private List<NavigationHistoryItem> _recentItems;
    private List<NavigationHistoryItem> _favorites;

    public NavigationHistoryService()
    {
        _localSettings = ApplicationData.Current.LocalSettings;
        _recentItems = LoadItems(RecentItemsKey);
        _favorites = LoadItems(FavoritesKey);
    }

    /// <summary>
    /// Gets recent navigation items (most recent first).
    /// </summary>
    public IReadOnlyList<NavigationHistoryItem> RecentItems => _recentItems.AsReadOnly();

    /// <summary>
    /// Gets favorite/pinned pages.
    /// </summary>
    public IReadOnlyList<NavigationHistoryItem> Favorites => _favorites.AsReadOnly();

    /// <summary>
    /// Records a page visit to recent history.
    /// </summary>
    public void RecordVisit(string pageName, string title, string? iconGlyph = null, object? parameter = null)
    {
        // Remove existing entry for same page
        _recentItems.RemoveAll(x => x.PageName == pageName && 
            (parameter == null || x.ParameterJson == JsonSerializer.Serialize(parameter)));

        // Add to front
        var item = new NavigationHistoryItem
        {
            PageName = pageName,
            Title = title,
            IconGlyph = iconGlyph ?? "\uE80F",
            ParameterJson = parameter != null ? JsonSerializer.Serialize(parameter) : null,
            VisitedAt = DateTime.Now
        };

        _recentItems.Insert(0, item);

        // Trim to max size
        if (_recentItems.Count > MaxRecentItems)
        {
            _recentItems = _recentItems.Take(MaxRecentItems).ToList();
        }

        SaveItems(RecentItemsKey, _recentItems);
    }

    /// <summary>
    /// Clears all recent items.
    /// </summary>
    public void ClearRecent()
    {
        _recentItems.Clear();
        SaveItems(RecentItemsKey, _recentItems);
    }

    /// <summary>
    /// Adds a page to favorites.
    /// </summary>
    public void AddFavorite(string pageName, string title, string? iconGlyph = null)
    {
        if (_favorites.Any(x => x.PageName == pageName))
            return;

        _favorites.Add(new NavigationHistoryItem
        {
            PageName = pageName,
            Title = title,
            IconGlyph = iconGlyph ?? "\uE734",
            VisitedAt = DateTime.Now
        });

        SaveItems(FavoritesKey, _favorites);
    }

    /// <summary>
    /// Removes a page from favorites.
    /// </summary>
    public void RemoveFavorite(string pageName)
    {
        _favorites.RemoveAll(x => x.PageName == pageName);
        SaveItems(FavoritesKey, _favorites);
    }

    /// <summary>
    /// Checks if a page is favorited.
    /// </summary>
    public bool IsFavorite(string pageName) => _favorites.Any(x => x.PageName == pageName);

    /// <summary>
    /// Toggles favorite status for a page.
    /// </summary>
    public bool ToggleFavorite(string pageName, string title, string? iconGlyph = null)
    {
        if (IsFavorite(pageName))
        {
            RemoveFavorite(pageName);
            return false;
        }
        else
        {
            AddFavorite(pageName, title, iconGlyph);
            return true;
        }
    }

    private List<NavigationHistoryItem> LoadItems(string key)
    {
        try
        {
            if (_localSettings.Values.TryGetValue(key, out var json) && json is string jsonString)
            {
                return JsonSerializer.Deserialize<List<NavigationHistoryItem>>(jsonString) ?? new List<NavigationHistoryItem>();
            }
        }
        catch
        {
            // Ignore deserialization errors
        }
        return new List<NavigationHistoryItem>();
    }

    private void SaveItems(string key, List<NavigationHistoryItem> items)
    {
        try
        {
            var json = JsonSerializer.Serialize(items);
            _localSettings.Values[key] = json;
        }
        catch
        {
            // Ignore serialization errors
        }
    }
}

/// <summary>
/// Represents a navigation history item.
/// </summary>
public class NavigationHistoryItem
{
    public string PageName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = "\uE80F";
    public string? ParameterJson { get; set; }
    public DateTime VisitedAt { get; set; }
}
