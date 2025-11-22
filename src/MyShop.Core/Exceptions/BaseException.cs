namespace MyShop.Core.Exceptions;

/// <summary>
/// Base exception class for all custom exceptions in the application
/// </summary>
public abstract class BaseException : Exception
{
    public string Code { get; }
    public DateTime Timestamp { get; }

    protected BaseException(string message, string code)
        : base(message)
    {
        Code = code;
        Timestamp = DateTime.UtcNow;
    }

    protected BaseException(string message, string code, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
        Timestamp = DateTime.UtcNow;
    }
}
