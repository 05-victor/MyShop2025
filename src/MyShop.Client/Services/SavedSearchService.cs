// ============================================================================
// SAVED SEARCH SERVICE
// File: Services/SavedSearchService.cs
// Description: Manages saved searches and filter presets
// ============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyShop.Client.Services;

/// <summary>
/// Service for saving and managing search queries and filter presets.
/// Allows users to save their frequently used search configurations.
/// </summary>
public class SavedSearchService : ISavedSearchService
{
    #region Fields

    private string _savedSearchesPath;
    private string _searchHistoryPath;
    private readonly ObservableCollection<SavedSearch> _savedSearches;
    private readonly ObservableCollection<SearchHistoryItem> _searchHistory;
    private readonly int _maxHistoryItems;
    private bool _isInitialized;

    #endregion

    #region Constructor

    public SavedSearchService(int maxHistoryItems = 50)
    {
        _maxHistoryItems = maxHistoryItems;
        _savedSearches = new ObservableCollection<SavedSearch>();
        _searchHistory = new ObservableCollection<SearchHistoryItem>();
        _isInitialized = false;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets all saved searches
    /// </summary>
    public ObservableCollection<SavedSearch> SavedSearches
    {
        get
        {
            EnsureInitialized();
            return _savedSearches;
        }
    }

    /// <summary>
    /// Gets search history
    /// </summary>
    public ObservableCollection<SearchHistoryItem> SearchHistory
    {
        get
        {
            EnsureInitialized();
            return _searchHistory;
        }
    }

    #endregion

    #region Public Methods - Saved Searches

    /// <summary>
    /// Ensures the service is initialized (lazy initialization)
    /// </summary>
    private void EnsureInitialized()
    {
        if (_isInitialized) return;

        try
        {
            // Set storage paths
            var localFolder = ApplicationData.Current.LocalFolder.Path;
            var appDataPath = Path.Combine(localFolder, "SavedSearches");
            Directory.CreateDirectory(appDataPath);

            _savedSearchesPath = Path.Combine(appDataPath, "saved_searches.json");
            _searchHistoryPath = Path.Combine(appDataPath, "search_history.json");

            _isInitialized = true;

            // Load saved data
            _ = LoadAllAsync();
        }
        catch (Exception ex)
        {
            // Fallback to temp directory
            var appDataPath = Path.Combine(Path.GetTempPath(), "MyShop", "SavedSearches");
            Directory.CreateDirectory(appDataPath);
            _savedSearchesPath = Path.Combine(appDataPath, "saved_searches.json");
            _searchHistoryPath = Path.Combine(appDataPath, "search_history.json");
            _isInitialized = true;
            System.Diagnostics.Debug.WriteLine($"SavedSearchService: Failed to use ApplicationData, using temp folder: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves a new search configuration
    /// </summary>
    public async Task<SavedSearch> SaveSearchAsync(
        string name,
        string context,
        string query = null,
        Dictionary<string, object> filters = null,
        string sortBy = null,
        bool sortDescending = false,
        bool isPinned = false)
    {
        EnsureInitialized();
        var savedSearch = new SavedSearch
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Context = context,
            Query = query,
            Filters = filters ?? new Dictionary<string, object>(),
            SortBy = sortBy,
            SortDescending = sortDescending,
            IsPinned = isPinned,
            CreatedAt = DateTime.Now,
            LastUsedAt = DateTime.Now,
            UseCount = 0
        };

        _savedSearches.Add(savedSearch);
        await SaveAllSearchesAsync();

        return savedSearch;
    }

    /// <summary>
    /// Updates an existing saved search
    /// </summary>
    public async Task<bool> UpdateSearchAsync(
        string id,
        string name = null,
        string query = null,
        Dictionary<string, object> filters = null,
        string sortBy = null,
        bool? sortDescending = null,
        bool? isPinned = null)
    {
        var search = _savedSearches.FirstOrDefault(s => s.Id == id);
        if (search == null) return false;

        if (name != null) search.Name = name;
        if (query != null) search.Query = query;
        if (filters != null) search.Filters = filters;
        if (sortBy != null) search.SortBy = sortBy;
        if (sortDescending.HasValue) search.SortDescending = sortDescending.Value;
        if (isPinned.HasValue) search.IsPinned = isPinned.Value;
        
        search.UpdatedAt = DateTime.Now;

        await SaveAllSearchesAsync();
        return true;
    }

    /// <summary>
    /// Deletes a saved search
    /// </summary>
    public async Task<bool> DeleteSearchAsync(string id)
    {
        var search = _savedSearches.FirstOrDefault(s => s.Id == id);
        if (search == null) return false;

        _savedSearches.Remove(search);
        await SaveAllSearchesAsync();
        return true;
    }

    /// <summary>
    /// Marks a saved search as used (updates stats)
    /// </summary>
    public async Task MarkSearchUsedAsync(string id)
    {
        var search = _savedSearches.FirstOrDefault(s => s.Id == id);
        if (search != null)
        {
            search.UseCount++;
            search.LastUsedAt = DateTime.Now;
            await SaveAllSearchesAsync();
        }
    }

    /// <summary>
    /// Toggles pin status of a saved search
    /// </summary>
    public async Task TogglePinAsync(string id)
    {
        var search = _savedSearches.FirstOrDefault(s => s.Id == id);
        if (search != null)
        {
            search.IsPinned = !search.IsPinned;
            await SaveAllSearchesAsync();
        }
    }

    /// <summary>
    /// Gets saved searches for a specific context (e.g., Products, Orders, Customers)
    /// </summary>
    public IEnumerable<SavedSearch> GetByContext(string context)
    {
        return _savedSearches
            .Where(s => s.Context.Equals(context, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.IsPinned)
            .ThenByDescending(s => s.LastUsedAt);
    }

    /// <summary>
    /// Gets pinned searches
    /// </summary>
    public IEnumerable<SavedSearch> GetPinned()
    {
        return _savedSearches.Where(s => s.IsPinned).OrderBy(s => s.Name);
    }

    /// <summary>
    /// Gets most frequently used searches
    /// </summary>
    public IEnumerable<SavedSearch> GetFrequentlyUsed(int limit = 5)
    {
        return _savedSearches
            .OrderByDescending(s => s.UseCount)
            .Take(limit);
    }

    /// <summary>
    /// Gets recently used searches
    /// </summary>
    public IEnumerable<SavedSearch> GetRecentlyUsed(int limit = 5)
    {
        return _savedSearches
            .OrderByDescending(s => s.LastUsedAt)
            .Take(limit);
    }

    /// <summary>
    /// Searches saved searches by name
    /// </summary>
    public IEnumerable<SavedSearch> Search(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return _savedSearches;

        return _savedSearches.Where(s => 
            s.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            (s.Query?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
    }

    #endregion

    #region Public Methods - Search History

    /// <summary>
    /// Adds a search to history
    /// </summary>
    public async Task AddToHistoryAsync(
        string context,
        string query,
        Dictionary<string, object> filters = null,
        int resultsCount = 0)
    {
        // Skip empty queries
        if (string.IsNullOrWhiteSpace(query) && (filters == null || filters.Count == 0))
            return;

        // Check for duplicate recent searches
        var existingIndex = _searchHistory
            .Take(10)
            .ToList()
            .FindIndex(h => 
                h.Context == context && 
                h.Query == query && 
                AreSameFilters(h.Filters, filters));

        if (existingIndex >= 0)
        {
            // Move existing to top and update
            var existing = _searchHistory[existingIndex];
            _searchHistory.RemoveAt(existingIndex);
            existing.Timestamp = DateTime.Now;
            existing.ResultsCount = resultsCount;
            _searchHistory.Insert(0, existing);
        }
        else
        {
            // Add new history item
            var historyItem = new SearchHistoryItem
            {
                Id = Guid.NewGuid().ToString(),
                Context = context,
                Query = query,
                Filters = filters ?? new Dictionary<string, object>(),
                Timestamp = DateTime.Now,
                ResultsCount = resultsCount
            };

            _searchHistory.Insert(0, historyItem);

            // Trim old history
            while (_searchHistory.Count > _maxHistoryItems)
            {
                _searchHistory.RemoveAt(_searchHistory.Count - 1);
            }
        }

        await SaveSearchHistoryAsync();
    }

    /// <summary>
    /// Gets search history for a context
    /// </summary>
    public IEnumerable<SearchHistoryItem> GetHistory(string context = null, int limit = 20)
    {
        var history = string.IsNullOrEmpty(context)
            ? _searchHistory
            : _searchHistory.Where(h => h.Context.Equals(context, StringComparison.OrdinalIgnoreCase));

        return history.Take(limit);
    }

    /// <summary>
    /// Clears search history
    /// </summary>
    public async Task ClearHistoryAsync(string context = null)
    {
        if (string.IsNullOrEmpty(context))
        {
            _searchHistory.Clear();
        }
        else
        {
            var toRemove = _searchHistory
                .Where(h => h.Context.Equals(context, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var item in toRemove)
            {
                _searchHistory.Remove(item);
            }
        }

        await SaveSearchHistoryAsync();
    }

    /// <summary>
    /// Removes a specific history item
    /// </summary>
    public async Task RemoveFromHistoryAsync(string id)
    {
        var item = _searchHistory.FirstOrDefault(h => h.Id == id);
        if (item != null)
        {
            _searchHistory.Remove(item);
            await SaveSearchHistoryAsync();
        }
    }

    /// <summary>
    /// Converts a history item to a saved search
    /// </summary>
    public async Task<SavedSearch> SaveFromHistoryAsync(string historyId, string name)
    {
        var historyItem = _searchHistory.FirstOrDefault(h => h.Id == historyId);
        if (historyItem == null) return null;

        return await SaveSearchAsync(
            name,
            historyItem.Context,
            historyItem.Query,
            historyItem.Filters);
    }

    #endregion

    #region Public Methods - Quick Filters

    /// <summary>
    /// Gets or creates default quick filters for a context
    /// </summary>
    public async Task<List<QuickFilter>> GetQuickFiltersAsync(string context)
    {
        // Return context-specific default filters
        var filters = context switch
        {
            "Products" => GetProductQuickFilters(),
            "Orders" => GetOrderQuickFilters(),
            "Customers" => GetCustomerQuickFilters(),
            "Inventory" => GetInventoryQuickFilters(),
            _ => new List<QuickFilter>()
        };

        return await Task.FromResult(filters);
    }

    private List<QuickFilter> GetProductQuickFilters()
    {
        return new List<QuickFilter>
        {
            new QuickFilter
            {
                Id = "products-in-stock",
                Name = "Còn hàng",
                Context = "Products",
                Icon = "\uE73E",
                Filters = new Dictionary<string, object> { ["InStock"] = true }
            },
            new QuickFilter
            {
                Id = "products-out-of-stock",
                Name = "Hết hàng",
                Context = "Products",
                Icon = "\uE783",
                Filters = new Dictionary<string, object> { ["InStock"] = false }
            },
            new QuickFilter
            {
                Id = "products-low-stock",
                Name = "Sắp hết hàng",
                Context = "Products",
                Icon = "\uE7BA",
                Filters = new Dictionary<string, object> { ["LowStock"] = true }
            },
            new QuickFilter
            {
                Id = "products-active",
                Name = "Đang bán",
                Context = "Products",
                Icon = "\uE73E",
                Filters = new Dictionary<string, object> { ["IsActive"] = true }
            },
            new QuickFilter
            {
                Id = "products-on-sale",
                Name = "Đang giảm giá",
                Context = "Products",
                Icon = "\uE8D2",
                Filters = new Dictionary<string, object> { ["OnSale"] = true }
            }
        };
    }

    private List<QuickFilter> GetOrderQuickFilters()
    {
        return new List<QuickFilter>
        {
            new QuickFilter
            {
                Id = "orders-today",
                Name = "Hôm nay",
                Context = "Orders",
                Icon = "\uE787",
                Filters = new Dictionary<string, object> { ["DateRange"] = "Today" }
            },
            new QuickFilter
            {
                Id = "orders-pending",
                Name = "Chờ xử lý",
                Context = "Orders",
                Icon = "\uE823",
                Filters = new Dictionary<string, object> { ["Status"] = "Pending" }
            },
            new QuickFilter
            {
                Id = "orders-processing",
                Name = "Đang xử lý",
                Context = "Orders",
                Icon = "\uE916",
                Filters = new Dictionary<string, object> { ["Status"] = "Processing" }
            },
            new QuickFilter
            {
                Id = "orders-completed",
                Name = "Hoàn thành",
                Context = "Orders",
                Icon = "\uE73E",
                Filters = new Dictionary<string, object> { ["Status"] = "Completed" }
            },
            new QuickFilter
            {
                Id = "orders-unpaid",
                Name = "Chưa thanh toán",
                Context = "Orders",
                Icon = "\uE8C7",
                Filters = new Dictionary<string, object> { ["PaymentStatus"] = "Unpaid" }
            }
        };
    }

    private List<QuickFilter> GetCustomerQuickFilters()
    {
        return new List<QuickFilter>
        {
            new QuickFilter
            {
                Id = "customers-new",
                Name = "Khách mới (30 ngày)",
                Context = "Customers",
                Icon = "\uE77B",
                Filters = new Dictionary<string, object> { ["NewCustomers"] = 30 }
            },
            new QuickFilter
            {
                Id = "customers-vip",
                Name = "Khách VIP",
                Context = "Customers",
                Icon = "\uE734",
                Filters = new Dictionary<string, object> { ["CustomerType"] = "VIP" }
            },
            new QuickFilter
            {
                Id = "customers-active",
                Name = "Hoạt động",
                Context = "Customers",
                Icon = "\uE73E",
                Filters = new Dictionary<string, object> { ["IsActive"] = true }
            },
            new QuickFilter
            {
                Id = "customers-with-orders",
                Name = "Đã đặt hàng",
                Context = "Customers",
                Icon = "\uE719",
                Filters = new Dictionary<string, object> { ["HasOrders"] = true }
            }
        };
    }

    private List<QuickFilter> GetInventoryQuickFilters()
    {
        return new List<QuickFilter>
        {
            new QuickFilter
            {
                Id = "inventory-low",
                Name = "Tồn kho thấp",
                Context = "Inventory",
                Icon = "\uE7BA",
                Filters = new Dictionary<string, object> { ["LowStock"] = true }
            },
            new QuickFilter
            {
                Id = "inventory-out",
                Name = "Hết hàng",
                Context = "Inventory",
                Icon = "\uE783",
                Filters = new Dictionary<string, object> { ["OutOfStock"] = true }
            },
            new QuickFilter
            {
                Id = "inventory-expiring",
                Name = "Sắp hết hạn",
                Context = "Inventory",
                Icon = "\uE823",
                Filters = new Dictionary<string, object> { ["Expiring"] = true }
            }
        };
    }

    #endregion

    #region Private Methods

    private async Task LoadAllAsync()
    {
        await LoadSavedSearchesAsync();
        await LoadSearchHistoryAsync();
    }

    private async Task LoadSavedSearchesAsync()
    {
        try
        {
            if (File.Exists(_savedSearchesPath))
            {
                var json = await File.ReadAllTextAsync(_savedSearchesPath);
                var searches = JsonSerializer.Deserialize<List<SavedSearch>>(json);
                
                _savedSearches.Clear();
                if (searches != null)
                {
                    foreach (var search in searches.OrderByDescending(s => s.IsPinned).ThenByDescending(s => s.LastUsedAt))
                    {
                        _savedSearches.Add(search);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load saved searches: {ex.Message}");
        }
    }

    private async Task SaveAllSearchesAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_savedSearches.ToList(), new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_savedSearchesPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save searches: {ex.Message}");
        }
    }

    private async Task LoadSearchHistoryAsync()
    {
        try
        {
            if (File.Exists(_searchHistoryPath))
            {
                var json = await File.ReadAllTextAsync(_searchHistoryPath);
                var history = JsonSerializer.Deserialize<List<SearchHistoryItem>>(json);
                
                _searchHistory.Clear();
                if (history != null)
                {
                    foreach (var item in history.OrderByDescending(h => h.Timestamp))
                    {
                        _searchHistory.Add(item);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load search history: {ex.Message}");
        }
    }

    private async Task SaveSearchHistoryAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_searchHistory.ToList(), new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_searchHistoryPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save search history: {ex.Message}");
        }
    }

    private bool AreSameFilters(Dictionary<string, object> filters1, Dictionary<string, object> filters2)
    {
        if (filters1 == null && filters2 == null) return true;
        if (filters1 == null || filters2 == null) return false;
        if (filters1.Count != filters2.Count) return false;

        foreach (var kvp in filters1)
        {
            if (!filters2.TryGetValue(kvp.Key, out var value2))
                return false;

            if (!Equals(kvp.Value?.ToString(), value2?.ToString()))
                return false;
        }

        return true;
    }

    #endregion
}

#region Interfaces

public interface ISavedSearchService
{
    ObservableCollection<SavedSearch> SavedSearches { get; }
    ObservableCollection<SearchHistoryItem> SearchHistory { get; }

    // Saved searches
    Task<SavedSearch> SaveSearchAsync(string name, string context, string query = null, 
        Dictionary<string, object> filters = null, string sortBy = null, 
        bool sortDescending = false, bool isPinned = false);
    Task<bool> UpdateSearchAsync(string id, string name = null, string query = null, 
        Dictionary<string, object> filters = null, string sortBy = null, 
        bool? sortDescending = null, bool? isPinned = null);
    Task<bool> DeleteSearchAsync(string id);
    Task MarkSearchUsedAsync(string id);
    Task TogglePinAsync(string id);
    IEnumerable<SavedSearch> GetByContext(string context);
    IEnumerable<SavedSearch> GetPinned();
    IEnumerable<SavedSearch> GetFrequentlyUsed(int limit = 5);
    IEnumerable<SavedSearch> GetRecentlyUsed(int limit = 5);
    IEnumerable<SavedSearch> Search(string searchTerm);

    // Search history
    Task AddToHistoryAsync(string context, string query, Dictionary<string, object> filters = null, int resultsCount = 0);
    IEnumerable<SearchHistoryItem> GetHistory(string context = null, int limit = 20);
    Task ClearHistoryAsync(string context = null);
    Task RemoveFromHistoryAsync(string id);
    Task<SavedSearch> SaveFromHistoryAsync(string historyId, string name);

    // Quick filters
    Task<List<QuickFilter>> GetQuickFiltersAsync(string context);
}

#endregion

#region Models

/// <summary>
/// Represents a saved search configuration
/// </summary>
public class SavedSearch
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Context { get; set; }
    public string Query { get; set; }
    public Dictionary<string, object> Filters { get; set; } = new();
    public string SortBy { get; set; }
    public bool SortDescending { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime LastUsedAt { get; set; }
    public int UseCount { get; set; }

    /// <summary>
    /// Gets display text for the search
    /// </summary>
    public string DisplayText
    {
        get
        {
            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(Query))
                parts.Add($"\"{Query}\"");
            
            if (Filters?.Count > 0)
                parts.Add($"{Filters.Count} bộ lọc");
            
            if (!string.IsNullOrEmpty(SortBy))
                parts.Add($"Sắp xếp: {SortBy}");

            return parts.Count > 0 ? string.Join(" • ", parts) : "Không có bộ lọc";
        }
    }

    /// <summary>
    /// Gets icon based on context
    /// </summary>
    public string Icon => Context switch
    {
        "Products" => "\uE7BF",     // Product icon
        "Orders" => "\uE719",       // Order icon
        "Customers" => "\uE77B",    // Customer icon
        "Inventory" => "\uE7B8",    // Inventory icon
        "Reports" => "\uE9F9",      // Report icon
        _ => "\uE721"               // Search icon
    };
}

/// <summary>
/// Represents a search history item
/// </summary>
public class SearchHistoryItem
{
    public string Id { get; set; }
    public string Context { get; set; }
    public string Query { get; set; }
    public Dictionary<string, object> Filters { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public int ResultsCount { get; set; }

    /// <summary>
    /// Gets display text for the history item
    /// </summary>
    public string DisplayText
    {
        get
        {
            if (!string.IsNullOrEmpty(Query))
                return Query;
            
            if (Filters?.Count > 0)
                return $"{Filters.Count} bộ lọc đã áp dụng";
            
            return "Tìm kiếm";
        }
    }

    /// <summary>
    /// Gets time ago display
    /// </summary>
    public string TimeAgo
    {
        get
        {
            var diff = DateTime.Now - Timestamp;
            if (diff.TotalMinutes < 1) return "Vừa xong";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";
            return Timestamp.ToString("dd/MM/yyyy");
        }
    }
}

/// <summary>
/// Represents a quick filter preset
/// </summary>
public class QuickFilter
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Context { get; set; }
    public string Icon { get; set; }
    public Dictionary<string, object> Filters { get; set; } = new();
    public bool IsActive { get; set; }
}

#endregion
