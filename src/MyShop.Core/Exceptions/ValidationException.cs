namespace MyShop.Core.Exceptions;

/// <summary>
/// Exception for validation errors
/// </summary>
public class ValidationException : BaseException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message, Dictionary<string, string[]>? errors = null)
        : base(message, "VALIDATION_ERROR")
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred", "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public static ValidationException RequiredField(string fieldName)
        => new($"{fieldName} is required");

    public static ValidationException InvalidFormat(string fieldName)
        => new($"{fieldName} has an invalid format");
}
