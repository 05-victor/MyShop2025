using System;
using System.Collections.Generic;
using System.Text.Json;
using Windows.Storage;

namespace MyShop.Client.Services;

/// <summary>
/// Service for auto-saving form drafts to persist user input across sessions.
/// </summary>
public class FormDraftService
{
    private const string DraftKeyPrefix = "FormDraft_";
    private readonly ApplicationDataContainer _localSettings;
    private readonly TimeSpan _autoSaveInterval = TimeSpan.FromSeconds(30);
    private readonly Dictionary<string, DateTime> _lastSaveTimes = new();

    public FormDraftService()
    {
        _localSettings = ApplicationData.Current.LocalSettings;
    }

    /// <summary>
    /// Saves form data as a draft.
    /// </summary>
    /// <typeparam name="T">Type of form data</typeparam>
    /// <param name="formId">Unique identifier for the form</param>
    /// <param name="data">Form data to save</param>
    /// <param name="throttle">If true, respects auto-save interval</param>
    /// <returns>True if saved, false if throttled</returns>
    public bool SaveDraft<T>(string formId, T data, bool throttle = true) where T : class
    {
        if (throttle && _lastSaveTimes.TryGetValue(formId, out var lastSave))
        {
            if (DateTime.Now - lastSave < _autoSaveInterval)
                return false;
        }

        try
        {
            var key = DraftKeyPrefix + formId;
            var draft = new FormDraft
            {
                Data = JsonSerializer.Serialize(data),
                SavedAt = DateTime.Now,
                TypeName = typeof(T).FullName ?? typeof(T).Name
            };
            
            _localSettings.Values[key] = JsonSerializer.Serialize(draft);
            _lastSaveTimes[formId] = DateTime.Now;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Loads a form draft if available.
    /// </summary>
    /// <typeparam name="T">Type of form data</typeparam>
    /// <param name="formId">Unique identifier for the form</param>
    /// <returns>Form data if found, null otherwise</returns>
    public T? LoadDraft<T>(string formId) where T : class
    {
        try
        {
            var key = DraftKeyPrefix + formId;
            if (_localSettings.Values.TryGetValue(key, out var value) && value is string json)
            {
                var draft = JsonSerializer.Deserialize<FormDraft>(json);
                if (draft?.Data != null)
                {
                    return JsonSerializer.Deserialize<T>(draft.Data);
                }
            }
        }
        catch
        {
            // Ignore deserialization errors
        }
        return null;
    }

    /// <summary>
    /// Gets the save timestamp of a draft.
    /// </summary>
    public DateTime? GetDraftSaveTime(string formId)
    {
        try
        {
            var key = DraftKeyPrefix + formId;
            if (_localSettings.Values.TryGetValue(key, out var value) && value is string json)
            {
                var draft = JsonSerializer.Deserialize<FormDraft>(json);
                return draft?.SavedAt;
            }
        }
        catch
        {
            // Ignore errors
        }
        return null;
    }

    /// <summary>
    /// Checks if a draft exists for a form.
    /// </summary>
    public bool HasDraft(string formId)
    {
        var key = DraftKeyPrefix + formId;
        return _localSettings.Values.ContainsKey(key);
    }

    /// <summary>
    /// Clears a form draft (usually after successful submit).
    /// </summary>
    public void ClearDraft(string formId)
    {
        var key = DraftKeyPrefix + formId;
        _localSettings.Values.Remove(key);
        _lastSaveTimes.Remove(formId);
    }

    /// <summary>
    /// Clears all form drafts.
    /// </summary>
    public void ClearAllDrafts()
    {
        var keysToRemove = new List<string>();
        foreach (var key in _localSettings.Values.Keys)
        {
            if (key.StartsWith(DraftKeyPrefix))
            {
                keysToRemove.Add(key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            _localSettings.Values.Remove(key);
        }
        
        _lastSaveTimes.Clear();
    }
}

/// <summary>
/// Internal class for storing draft metadata.
/// </summary>
internal class FormDraft
{
    public string? Data { get; set; }
    public DateTime SavedAt { get; set; }
    public string? TypeName { get; set; }
}
