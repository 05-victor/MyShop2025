using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Client.Views.Components.Badges;

public enum StatusBadgeVariant
{
    Default,
    Pending,
    Approved,
    Rejected,
    Processing,
    Completed,
    Info,
    // Keep old variants for compatibility
    Success,
    Warning,
    Error,
    Primary
}

/// <summary>
/// Status badge component with color variants.
/// Usage:
/// <badges:StatusBadge Text="Success" Variant="Success"/>
/// <badges:StatusBadge Text="Pending" Variant="Pending" ShowIcon="True"/>
/// </summary>
public sealed partial class StatusBadge : UserControl
{
    public StatusBadge()
    {
        InitializeComponent();
        // Don't call UpdateBadgeStyle() here - it will be called by OnVariantChanged when properties are set
        // Calling it here causes ArgumentException because WinRT hasn't fully initialized named elements yet
        Loaded += (s, e) => UpdateBadgeStyle(); // Update on Loaded event instead
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StatusBadge),
            new PropertyMetadata(string.Empty, OnTextChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Don't auto-detect variant from text anymore
    }

    private StatusBadgeVariant DetectVariantFromText(string text)
    {
        if (string.IsNullOrEmpty(text)) return StatusBadgeVariant.Default;

        var lowerText = text.ToLower();
        
        // Pending patterns
        if (lowerText.Contains("pending") || lowerText.Contains("chờ") || 
            lowerText.Contains("chờ xử lý") || lowerText.Contains("chờ duyệt"))
            return StatusBadgeVariant.Pending;
        
        // Approved patterns
        if (lowerText.Contains("approved") || lowerText.Contains("đã duyệt") ||
            lowerText.Contains("active") || lowerText.Contains("success"))
            return StatusBadgeVariant.Approved;
        
        // Rejected patterns
        if (lowerText.Contains("rejected") || lowerText.Contains("từ chối") ||
            lowerText.Contains("error") || lowerText.Contains("failed") || 
            lowerText.Contains("inactive"))
            return StatusBadgeVariant.Rejected;
        
        // Processing patterns
        if (lowerText.Contains("processing") || lowerText.Contains("đang xử lý") ||
            lowerText.Contains("confirmed") || lowerText.Contains("đã xác nhận") ||
            lowerText.Contains("shipped") || lowerText.Contains("đang giao") ||
            lowerText.Contains("in progress") || lowerText.Contains("warning"))
            return StatusBadgeVariant.Processing;
        
        // Completed patterns
        if (lowerText.Contains("completed") || lowerText.Contains("hoàn thành") ||
            lowerText.Contains("delivered") || lowerText.Contains("đã giao"))
            return StatusBadgeVariant.Completed;
        
        // Info patterns
        if (lowerText.Contains("info") || lowerText.Contains("thông tin"))
            return StatusBadgeVariant.Info;

        return StatusBadgeVariant.Default;
    }

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(
            nameof(Variant),
            typeof(StatusBadgeVariant),
            typeof(StatusBadge),
            new PropertyMetadata(StatusBadgeVariant.Default, OnVariantChanged));

    public StatusBadgeVariant Variant
    {
        get => (StatusBadgeVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public static readonly DependencyProperty ShowIconProperty =
        DependencyProperty.Register(
            nameof(ShowIcon),
            typeof(bool),
            typeof(StatusBadge),
            new PropertyMetadata(false, OnShowIconChanged));

    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusBadge badge)
        {
            System.Diagnostics.Debug.WriteLine($"[StatusBadge] Variant changed: {e.OldValue} → {e.NewValue}");
            badge.UpdateBadgeStyle();
        }
    }

    private static void OnShowIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusBadge badge && badge.BadgeIcon != null)
        {
            badge.BadgeIcon.Visibility = badge.ShowIcon ? Visibility.Visible : Visibility.Collapsed;
            badge.UpdateBadgeStyle();
        }
    }

    private void UpdateBadgeStyle()
    {
        // Safety check: ensure all named elements are initialized
        if (BadgeContainer == null || BadgeText == null || BadgeIcon == null)
        {
            System.Diagnostics.Debug.WriteLine($"[StatusBadge] UpdateBadgeStyle skipped - elements not initialized");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[StatusBadge] UpdateBadgeStyle called for Variant: {Variant}");

        // Map variants to resource keys and icons
        var (bgKey, fgKey, iconGlyph) = Variant switch
        {
            // New semantic variants (use ThemeResource)
            StatusBadgeVariant.Pending => ("PendingBackgroundBrush", "PendingForegroundBrush", "\uE9F5"),
            StatusBadgeVariant.Approved => ("ApprovedBackgroundBrush", "ApprovedForegroundBrush", "\uE73E"),
            StatusBadgeVariant.Rejected => ("RejectedBackgroundBrush", "RejectedForegroundBrush", "\uE711"),
            StatusBadgeVariant.Processing => ("ProcessingBackgroundBrush", "ProcessingForegroundBrush", "\uE916"),
            StatusBadgeVariant.Completed => ("CompletedBackgroundBrush", "CompletedForegroundBrush", "\uE73E"),
            StatusBadgeVariant.Info => ("InfoBackgroundBrush", "InfoForegroundBrush", "\uE946"),
            
            // Old variants for backward compatibility (map to new variants)
            StatusBadgeVariant.Success => ("ApprovedBackgroundBrush", "ApprovedForegroundBrush", "\uE73E"),
            StatusBadgeVariant.Warning => ("PendingBackgroundBrush", "PendingForegroundBrush", "\uE7BA"),
            StatusBadgeVariant.Error => ("RejectedBackgroundBrush", "RejectedForegroundBrush", "\uE783"),
            StatusBadgeVariant.Primary => ("ProcessingBackgroundBrush", "ProcessingForegroundBrush", "\uE735"),
            
            // Default
            _ => ("CardBackgroundFillColorDefaultBrush", "TextFillColorPrimaryBrush", "\uE734")
        };

        System.Diagnostics.Debug.WriteLine($"[StatusBadge] Resource keys: BG={bgKey}, FG={fgKey}");

        // CRITICAL FIX: Use FrameworkElement.Resources to get theme-aware brushes
        // Application.Current.Resources doesn't resolve ThemeDictionaries!
        // We need to get resources from the element's actual theme context
        var bgBrush = GetThemeResource(bgKey);
        var fgBrush = GetThemeResource(fgKey);

        if (bgBrush is Brush brush)
        {
            BadgeContainer.Background = brush;
            
            // Log actual color value
            if (brush is SolidColorBrush solidBg)
            {
                var color = solidBg.Color;
                System.Diagnostics.Debug.WriteLine($"[StatusBadge] ✅ Background APPLIED: #{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[StatusBadge] ❌ Background brush NOT FOUND for key: {bgKey}");
        }

        if (fgBrush is Brush fgBrushCast)
        {
            BadgeText.Foreground = fgBrushCast;
            BadgeIcon.Foreground = fgBrushCast;
            
            // Log actual color value
            if (fgBrushCast is SolidColorBrush solidFg)
            {
                var color = solidFg.Color;
                System.Diagnostics.Debug.WriteLine($"[StatusBadge] ✅ Foreground APPLIED: #{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[StatusBadge] ❌ Foreground brush NOT FOUND for key: {fgKey}");
        }

        // Set icon
        if (!string.IsNullOrEmpty(iconGlyph))
        {
            BadgeIcon.Glyph = iconGlyph;
        }
        
        System.Diagnostics.Debug.WriteLine($"[StatusBadge] UpdateBadgeStyle complete");
    }

    /// <summary>
    /// Gets a theme-aware resource by traversing the visual tree to find ThemeDictionaries.
    /// This is necessary because Application.Current.Resources doesn't resolve ThemeDictionaries.
    /// </summary>
    private object? GetThemeResource(string key)
    {
        // First try to get from the element's own Resources
        if (Resources.TryGetValue(key, out var localResource))
            return localResource;

        // Try to get from App Resources with ThemeDictionaries
        var appResources = Application.Current.Resources;
        
        // Get current theme key
        var themeKey = ActualTheme == ElementTheme.Dark ? "Dark" : "Light";
        
        // Try ThemeDictionaries first
        if (appResources.ThemeDictionaries.TryGetValue(themeKey, out var themeDict) && 
            themeDict is ResourceDictionary dict &&
            dict.TryGetValue(key, out var themeResource))
        {
            return themeResource;
        }
        
        // Fallback to merged dictionaries
        if (appResources.TryGetValue(key, out var mergedResource))
            return mergedResource;

        return null;
    }
}
