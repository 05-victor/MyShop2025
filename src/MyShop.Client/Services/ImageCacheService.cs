using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MyShop.Client.Services;

/// <summary>
/// Interface for image caching service with memory and disk caching support.
/// </summary>
public interface IImageCacheService
{
    /// <summary>
    /// Gets an image from cache or loads it from the specified URL.
    /// </summary>
    /// <param name="url">The image URL to load.</param>
    /// <param name="decodePixelWidth">Optional decode pixel width for memory optimization.</param>
    /// <returns>A BitmapImage or null if loading fails.</returns>
    Task<BitmapImage?> GetImageAsync(string url, int decodePixelWidth = 0);
    
    /// <summary>
    /// Gets an image synchronously from memory cache only.
    /// </summary>
    /// <param name="url">The image URL.</param>
    /// <param name="decodePixelWidth">The decode pixel width used as cache key.</param>
    /// <returns>A BitmapImage if in cache, null otherwise.</returns>
    BitmapImage? GetFromMemoryCache(string url, int decodePixelWidth = 0);
    
    /// <summary>
    /// Preloads images into memory cache for faster subsequent access.
    /// </summary>
    /// <param name="urls">Collection of image URLs to preload.</param>
    /// <param name="decodePixelWidth">Optional decode pixel width.</param>
    Task PreloadImagesAsync(IEnumerable<string> urls, int decodePixelWidth = 0);
    
    /// <summary>
    /// Clears all cached images from memory.
    /// </summary>
    void ClearMemoryCache();
    
    /// <summary>
    /// Clears all cached images from disk.
    /// </summary>
    Task ClearDiskCacheAsync();
    
    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    ImageCacheStats GetStats();
}

/// <summary>
/// Image caching service with in-memory and optional disk caching.
/// Optimizes image loading with DecodePixelWidth for memory efficiency.
/// </summary>
public class ImageCacheService : IImageCacheService
{
    private readonly ConcurrentDictionary<string, BitmapImage> _memoryCache = new();
    private readonly ConcurrentDictionary<string, DateTime> _cacheTimestamps = new();
    private readonly string _cacheFolder;
    private readonly TimeSpan _memoryCacheExpiry = TimeSpan.FromMinutes(30);
    private readonly TimeSpan _diskCacheExpiry = TimeSpan.FromDays(7);
    private readonly int _maxMemoryCacheItems = 100;
    private int _cacheHits;
    private int _cacheMisses;

    public ImageCacheService()
    {
        try
        {
            _cacheFolder = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "ImageCache");
            Directory.CreateDirectory(_cacheFolder);
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Warning($"Failed to create image cache folder: {ex.Message}");
            _cacheFolder = string.Empty;
        }
    }

    /// <summary>
    /// Gets an image from cache or loads it from the specified URL.
    /// </summary>
    public async Task<BitmapImage?> GetImageAsync(string url, int decodePixelWidth = 0)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        var cacheKey = GenerateCacheKey(url, decodePixelWidth);

        // Check memory cache first
        if (_memoryCache.TryGetValue(cacheKey, out var cachedImage))
        {
            if (_cacheTimestamps.TryGetValue(cacheKey, out var timestamp) && 
                DateTime.UtcNow - timestamp < _memoryCacheExpiry)
            {
                _cacheHits++;
                return cachedImage;
            }
            else
            {
                // Expired - remove from cache
                _memoryCache.TryRemove(cacheKey, out _);
                _cacheTimestamps.TryRemove(cacheKey, out _);
            }
        }

        _cacheMisses++;

        try
        {
            var bitmap = new BitmapImage();
            
            // Set decode pixel width for memory optimization
            if (decodePixelWidth > 0)
            {
                bitmap.DecodePixelWidth = decodePixelWidth;
                bitmap.DecodePixelType = DecodePixelType.Logical;
            }

            // Set the URI source - WinUI handles the loading
            bitmap.UriSource = new Uri(url);
            
            // Manage cache size before adding
            if (_memoryCache.Count >= _maxMemoryCacheItems)
            {
                EvictOldestEntries();
            }

            // Cache in memory
            _memoryCache[cacheKey] = bitmap;
            _cacheTimestamps[cacheKey] = DateTime.UtcNow;
            
            return bitmap;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Warning($"Failed to load image: {url} - {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets an image synchronously from memory cache only.
    /// </summary>
    public BitmapImage? GetFromMemoryCache(string url, int decodePixelWidth = 0)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        var cacheKey = GenerateCacheKey(url, decodePixelWidth);

        if (_memoryCache.TryGetValue(cacheKey, out var cachedImage))
        {
            if (_cacheTimestamps.TryGetValue(cacheKey, out var timestamp) && 
                DateTime.UtcNow - timestamp < _memoryCacheExpiry)
            {
                _cacheHits++;
                return cachedImage;
            }
        }

        return null;
    }

    /// <summary>
    /// Preloads images into memory cache for faster subsequent access.
    /// </summary>
    public async Task PreloadImagesAsync(IEnumerable<string> urls, int decodePixelWidth = 0)
    {
        var tasks = urls
            .Where(url => !string.IsNullOrEmpty(url))
            .Select(url => GetImageAsync(url, decodePixelWidth));
        
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Clears all cached images from memory.
    /// </summary>
    public void ClearMemoryCache()
    {
        _memoryCache.Clear();
        _cacheTimestamps.Clear();
        _cacheHits = 0;
        _cacheMisses = 0;
        
        LoggingService.Instance.Information("Image memory cache cleared");
    }

    /// <summary>
    /// Clears all cached images from disk.
    /// </summary>
    public async Task ClearDiskCacheAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_cacheFolder) && Directory.Exists(_cacheFolder))
            {
                await Task.Run(() =>
                {
                    Directory.Delete(_cacheFolder, true);
                    Directory.CreateDirectory(_cacheFolder);
                });
                
                LoggingService.Instance.Information("Image disk cache cleared");
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Warning($"Failed to clear disk cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    public ImageCacheStats GetStats()
    {
        return new ImageCacheStats
        {
            MemoryCacheCount = _memoryCache.Count,
            CacheHits = _cacheHits,
            CacheMisses = _cacheMisses,
            HitRate = _cacheHits + _cacheMisses > 0 
                ? (double)_cacheHits / (_cacheHits + _cacheMisses) * 100 
                : 0
        };
    }

    /// <summary>
    /// Generates a cache key from URL and decode pixel width.
    /// </summary>
    private static string GenerateCacheKey(string url, int decodePixelWidth)
    {
        return decodePixelWidth > 0 ? $"{url}_{decodePixelWidth}" : url;
    }

    /// <summary>
    /// Evicts the oldest entries from memory cache.
    /// </summary>
    private void EvictOldestEntries()
    {
        // Remove the oldest 20% of entries
        var entriesToRemove = _cacheTimestamps
            .OrderBy(kvp => kvp.Value)
            .Take(_maxMemoryCacheItems / 5)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in entriesToRemove)
        {
            _memoryCache.TryRemove(key, out _);
            _cacheTimestamps.TryRemove(key, out _);
        }
    }
}

/// <summary>
/// Image cache statistics.
/// </summary>
public class ImageCacheStats
{
    /// <summary>
    /// Number of images currently in memory cache.
    /// </summary>
    public int MemoryCacheCount { get; set; }
    
    /// <summary>
    /// Number of cache hits.
    /// </summary>
    public int CacheHits { get; set; }
    
    /// <summary>
    /// Number of cache misses.
    /// </summary>
    public int CacheMisses { get; set; }
    
    /// <summary>
    /// Cache hit rate percentage.
    /// </summary>
    public double HitRate { get; set; }
}
