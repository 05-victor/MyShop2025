namespace MyShop.Core.Common;

/// <summary>
/// Represents a void type for Result pattern
/// Used when operation returns no data (only success/failure)
/// </summary>
public readonly struct Unit
{
    public static readonly Unit Value = new();
}
