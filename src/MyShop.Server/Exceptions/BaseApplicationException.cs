namespace MyShop.Server.Exceptions;

/// <summary>
/// Base exception class for all custom exceptions in MyShop application
/// Provides common properties for error handling and logging
/// </summary>
public abstract class BaseApplicationException : Exception
{
    /// <summary>
    /// Error code for categorizing exceptions
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// HTTP status code to return
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Timestamp when exception occurred
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Additional error details (optional, for debugging)
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }

    protected BaseApplicationException(
        string message, 
        string errorCode, 
        int statusCode,
        Exception? innerException = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Add additional context data to the exception
    /// </summary>
    public void AddData(string key, object value)
    {
        AdditionalData ??= new Dictionary<string, object>();
        AdditionalData[key] = value;
    }
}
