namespace MyShop.Server.Exceptions;

/// <summary>
/// Exception thrown when authentication or authorization fails
/// Maps to 401 Unauthorized or 403 Forbidden HTTP status
/// </summary>
public class AuthenticationException : BaseApplicationException
{
    public AuthenticationException(string message, int statusCode = StatusCodes.Status401Unauthorized)
        : base(message, "AUTHENTICATION_ERROR", statusCode)
    {
    }

    public AuthenticationException(string message, Exception innerException, int statusCode = StatusCodes.Status401Unauthorized)
        : base(message, "AUTHENTICATION_ERROR", statusCode, innerException)
    {
    }

    /// <summary>
    /// Create an AuthenticationException for invalid credentials
    /// </summary>
    public static AuthenticationException InvalidCredentials()
        => new("Invalid username or password");

    /// <summary>
    /// Create an AuthenticationException for expired tokens
    /// </summary>
    public static AuthenticationException ExpiredToken()
        => new("Your session has expired. Please login again");

    /// <summary>
    /// Create an AuthenticationException for forbidden access (403)
    /// </summary>
    public static AuthenticationException Forbidden(string? resource = null)
    {
        var message = string.IsNullOrEmpty(resource)
            ? "You do not have permission to access this resource"
            : $"You do not have permission to access {resource}";
        return new AuthenticationException(message, StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// Create an AuthenticationException for missing authentication
    /// </summary>
    public static AuthenticationException Unauthenticated()
        => new("Authentication required. Please login to continue");
}
