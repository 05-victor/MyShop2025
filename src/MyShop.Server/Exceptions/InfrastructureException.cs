namespace MyShop.Server.Exceptions;

/// <summary>
/// Exception thrown when database or external service operations fail
/// Maps to 500 Internal Server Error or 503 Service Unavailable
/// </summary>
public class InfrastructureException : BaseApplicationException
{
    public InfrastructureException(string message, Exception? innerException = null, int statusCode = StatusCodes.Status500InternalServerError)
        : base(message, "INFRASTRUCTURE_ERROR", statusCode, innerException)
    {
    }

    /// <summary>
    /// Create an InfrastructureException for database errors
    /// </summary>
    public static InfrastructureException DatabaseError(string message, Exception innerException)
        => new($"Database error: {message}", innerException);

    /// <summary>
    /// Create an InfrastructureException for service unavailability (503)
    /// </summary>
    public static InfrastructureException ServiceUnavailable(string serviceName)
        => new(
            $"Service '{serviceName}' is currently unavailable. Please try again later",
            null,
            StatusCodes.Status503ServiceUnavailable);

    /// <summary>
    /// Create an InfrastructureException for external API errors
    /// </summary>
    public static InfrastructureException ExternalApiError(string apiName, Exception innerException)
        => new($"External API '{apiName}' error", innerException);
}
