using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Services;

/// <summary>
/// Service interface for search autocomplete and suggestions
/// Aggregates product search, category search, and search history
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Get autocomplete suggestions for search query
    /// Combines: recent searches, product matches, category matches
    /// </summary>
    /// <param name="query">Search query (minimum 2 characters)</param>
    /// <param name="maxResults">Maximum suggestions to return (default 10)</param>
    /// <returns>Grouped search suggestions</returns>
    Task<Result<SearchSuggestions>> GetSuggestionsAsync(string query, int maxResults = 10);

    /// <summary>
    /// Add search query to user's search history
    /// </summary>
    Task<Result<Unit>> AddToHistoryAsync(string query);

    /// <summary>
    /// Get user's recent search history
    /// </summary>
    Task<Result<IEnumerable<string>>> GetSearchHistoryAsync(int maxItems = 5);

    /// <summary>
    /// Clear user's search history
    /// </summary>
    Task<Result<Unit>> ClearHistoryAsync();
}

/// <summary>
/// Container for grouped search suggestions
/// </summary>
public class SearchSuggestions
{
    /// <summary>
    /// Recent searches matching query (from search-suggestions.json)
    /// </summary>
    public List<string> RecentSearches { get; set; } = new();

    /// <summary>
    /// Product matches (from products.json)
    /// Name, Manufacturer, SKU
    /// </summary>
    public List<ProductSuggestion> ProductMatches { get; set; } = new();

    /// <summary>
    /// Category matches (from categories.json)
    /// </summary>
    public List<CategorySuggestion> CategoryMatches { get; set; } = new();

    /// <summary>
    /// Total suggestions count across all groups
    /// </summary>
    public int TotalCount => RecentSearches.Count + ProductMatches.Count + CategoryMatches.Count;

    /// <summary>
    /// Check if any suggestions exist
    /// </summary>
    public bool HasSuggestions => TotalCount > 0;
}

/// <summary>
/// Product search suggestion with thumbnail
/// </summary>
public class ProductSuggestion
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string DisplayText => $"{Name} - {Manufacturer}";
}

/// <summary>
/// Category search suggestion
/// </summary>
public class CategorySuggestion
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public string DisplayText => $"All {Name} ({ProductCount} products)";
}

