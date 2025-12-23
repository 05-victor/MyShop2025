using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.Services;

/// <summary>
/// Interface for in-memory cache service for API responses and computed data.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value or creates it using the async factory function.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    
    /// <summary>
    /// Gets a cached value or creates it using the sync factory function.
    /// </summary>
    T GetOrAdd<T>(string key, Func<T> factory, TimeSpan? expiration = null);
    
    /// <summary>
    /// Gets a cached value synchronously.
    /// </summary>
    T? Get<T>(string key);
    
    /// <summary>
    /// Tries to get a cached value.
    /// </summary>
    bool TryGet<T>(string key, out T? value);
    
    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    
    /// <summary>
    /// Checks if a key exists and is not expired.
    /// </summary>
    bool Contains(string key);
    
    /// <summary>
    /// Removes a specific key from the cache.
    /// </summary>
    void Remove(string key);
    
    /// <summary>
    /// Removes all keys matching a prefix.
    /// </summary>
    void RemoveByPrefix(string prefix);
    
    /// <summary>
    /// Clears all cached entries.
    /// </summary>
    void Clear();
    
    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    CacheStats GetStats();
    
    /// <summary>
    /// Cleans up expired entries.
    /// </summary>
    void CleanupExpired();
}

/// <summary>
/// In-memory cache service for API responses and computed data.
/// Supports expiration and manual invalidation.
/// </summary>
public class CacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets a cached value or creates it using the factory function.
    /// </summary>
    /// <typeparam name="T">Type of cached value</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Factory function to create value if not cached</param>
    /// <param name="expiration">Optional expiration time</param>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (!entry.IsExpired && entry.Value is T cachedValue)
            {
                return cachedValue;
            }
            
            // Remove expired entry
            _cache.TryRemove(key, out _);
        }

        // Create new value
        var value = await factory();
        var newEntry = new CacheEntry
        {
            Value = value,
            ExpiresAt = DateTime.UtcNow + (expiration ?? _defaultExpiration)
        };
        
        _cache.TryAdd(key, newEntry);
        return value;
    }

    /// <summary>
    /// Gets a cached value or creates it using the synchronous factory function.
    /// </summary>
    public T GetOrAdd<T>(string key, Func<T> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (!entry.IsExpired && entry.Value is T cachedValue)
            {
                return cachedValue;
            }
            
            // Remove expired entry
            _cache.TryRemove(key, out _);
        }

        // Create new value
        var value = factory();
        var newEntry = new CacheEntry
        {
            Value = value,
            ExpiresAt = DateTime.UtcNow + (expiration ?? _defaultExpiration)
        };
        
        _cache.TryAdd(key, newEntry);
        return value;
    }

    /// <summary>
    /// Gets a cached value synchronously.
    /// </summary>
    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired && entry.Value is T value)
        {
            return value;
        }
        return default;
    }

    /// <summary>
    /// Tries to get a cached value.
    /// </summary>
    public bool TryGet<T>(string key, out T? value)
    {
        if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired && entry.Value is T cachedValue)
        {
            value = cachedValue;
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var entry = new CacheEntry
        {
            Value = value,
            ExpiresAt = DateTime.UtcNow + (expiration ?? _defaultExpiration)
        };
        _cache[key] = entry;
    }

    /// <summary>
    /// Checks if a key exists and is not expired.
    /// </summary>
    public bool Contains(string key)
    {
        return _cache.TryGetValue(key, out var entry) && !entry.IsExpired;
    }

    /// <summary>
    /// Removes a specific key from the cache.
    /// </summary>
    public void Remove(string key)
    {
        _cache.TryRemove(key, out _);
    }

    /// <summary>
    /// Removes all keys matching a prefix.
    /// </summary>
    public void RemoveByPrefix(string prefix)
    {
        foreach (var key in _cache.Keys)
        {
            if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                _cache.TryRemove(key, out _);
            }
        }
    }

    /// <summary>
    /// Clears all cached entries.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    public CacheStats GetStats()
    {
        var validCount = 0;
        var expiredCount = 0;

        foreach (var entry in _cache.Values)
        {
            if (entry.IsExpired)
                expiredCount++;
            else
                validCount++;
        }

        return new CacheStats
        {
            TotalEntries = _cache.Count,
            ValidEntries = validCount,
            ExpiredEntries = expiredCount
        };
    }

    /// <summary>
    /// Cleans up expired entries.
    /// </summary>
    public void CleanupExpired()
    {
        foreach (var kvp in _cache)
        {
            if (kvp.Value.IsExpired)
            {
                _cache.TryRemove(kvp.Key, out _);
            }
        }
    }

    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }
}

/// <summary>
/// Cache statistics.
/// </summary>
public class CacheStats
{
    public int TotalEntries { get; set; }
    public int ValidEntries { get; set; }
    public int ExpiredEntries { get; set; }
}
