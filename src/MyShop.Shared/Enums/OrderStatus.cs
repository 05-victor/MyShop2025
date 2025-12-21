namespace MyShop.Shared.Enums;

/// <summary>
/// Order status enumeration
/// Stored as integer in database, converted to string for API/frontend
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Returned = 6
}
