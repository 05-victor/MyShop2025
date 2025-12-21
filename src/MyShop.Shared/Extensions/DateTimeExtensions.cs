namespace MyShop.Shared.Extensions;

/// <summary>
/// Extension methods for DateTime manipulation
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Convert DateTime to relative time string (e.g., "2 hours ago")
    /// </summary>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;

        if (timeSpan.TotalSeconds < 60)
            return "just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} week{(timeSpan.TotalDays >= 14 ? "s" : "")} ago";
        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} month{(timeSpan.TotalDays >= 60 ? "s" : "")} ago";

        return $"{(int)(timeSpan.TotalDays / 365)} year{(timeSpan.TotalDays >= 730 ? "s" : "")} ago";
    }

    /// <summary>
    /// Convert DateTime to short date string (e.g., "Nov 22, 2025")
    /// </summary>
    public static string ToShortDate(this DateTime dateTime)
    {
        return dateTime.ToString("MMM dd, yyyy");
    }

    /// <summary>
    /// Convert DateTime to short date and time string (e.g., "Nov 22, 2025 3:45 PM")
    /// </summary>
    public static string ToShortDateTime(this DateTime dateTime)
    {
        return dateTime.ToString("MMM dd, yyyy h:mm tt");
    }

    /// <summary>
    /// Check if DateTime is today
    /// </summary>
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today;
    }

    /// <summary>
    /// Check if DateTime is in the past
    /// </summary>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.Now;
    }

    /// <summary>
    /// Check if DateTime is in the future
    /// </summary>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.Now;
    }

    /// <summary>
    /// Get start of day (00:00:00) with UTC kind
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Get end of day (23:59:59) with UTC kind
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }
    
    /// <summary>
    /// Get start of month (1st day, 00:00:00) with UTC kind
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);
    }
    
    /// <summary>
    /// Get end of month (last day, 23:59:59) with UTC kind
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return StartOfMonth(dateTime).AddMonths(1).AddSeconds(-1);
    }
    
    /// <summary>
    /// Ensure DateTime has UTC kind (converts if needed)
    /// </summary>
    public static DateTime EnsureUtc(this DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
    }
}
