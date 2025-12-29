namespace MyShop.Core.Common;

/// <summary>
/// Result of bulk import operation
/// </summary>
public class BulkImportResult
{
    public int TotalSubmitted { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
