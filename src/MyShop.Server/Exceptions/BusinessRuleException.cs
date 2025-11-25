namespace MyShop.Server.Exceptions;

/// <summary>
/// Exception thrown when business logic rules are violated
/// Maps to 400 Bad Request or 409 Conflict HTTP status depending on the scenario
/// </summary>
public class BusinessRuleException : BaseApplicationException
{
    public BusinessRuleException(string message, int statusCode = StatusCodes.Status400BadRequest)
        : base(message, "BUSINESS_RULE_VIOLATION", statusCode)
    {
    }

    public BusinessRuleException(string message, Exception innerException, int statusCode = StatusCodes.Status400BadRequest)
        : base(message, "BUSINESS_RULE_VIOLATION", statusCode, innerException)
    {
    }

    /// <summary>
    /// Create a BusinessRuleException for conflict scenarios (409)
    /// </summary>
    public static BusinessRuleException Conflict(string message)
        => new(message, StatusCodes.Status409Conflict);

    /// <summary>
    /// Create a BusinessRuleException for invalid operations
    /// </summary>
    public static BusinessRuleException InvalidOperation(string message)
        => new($"Invalid operation: {message}");
}
