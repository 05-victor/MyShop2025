namespace MyShop.Shared.Enums;

/// <summary>
/// Payment status enumeration
/// Stored as integer in database, converted to string for API/frontend
/// </summary>
public enum PaymentStatus
{
    Unpaid = 0,
    Paid = 1,
    PartiallyPaid = 2,
    Refunded = 3,
    Failed = 4
}
