namespace MyShop.Shared.Enums;

/// <summary>
/// Product status enumeration
/// Stored as integer in database, converted to string for API/frontend
/// </summary>
public enum ProductStatus
{
    Available = 0,
    OutOfStock = 1,
    Discontinued = 2,
    Pending = 3
}
