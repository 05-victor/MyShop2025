namespace MyShop.Shared.Enums;

/// <summary>
/// Agent request status enumeration
/// Stored as integer in database, converted to string for API/frontend
/// </summary>
public enum AgentRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
