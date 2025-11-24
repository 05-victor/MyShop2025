namespace MyShop.Server.Exceptions;

/// <summary>
/// Exception thrown when business validation rules are violated
/// Maps to 400 Bad Request HTTP status
/// </summary>
public class ValidationException : BaseApplicationException
{
    /// <summary>
    /// Dictionary of field names and their validation errors
    /// </summary>
    public Dictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(string message)
        : base(message, "VALIDATION_ERROR", StatusCodes.Status400BadRequest)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }

    public ValidationException(Dictionary<string, string[]> validationErrors)
        : base("One or more validation errors occurred", "VALIDATION_ERROR", StatusCodes.Status400BadRequest)
    {
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Add a validation error for a specific field
    /// </summary>
    public void AddError(string fieldName, string errorMessage)
    {
        if (!ValidationErrors.ContainsKey(fieldName))
        {
            ValidationErrors[fieldName] = new[] { errorMessage };
        }
        else
        {
            var errors = ValidationErrors[fieldName].ToList();
            errors.Add(errorMessage);
            ValidationErrors[fieldName] = errors.ToArray();
        }
    }

    /// <summary>
    /// Create a ValidationException with a single field error
    /// </summary>
    public static ValidationException ForField(string fieldName, string errorMessage)
    {
        var exception = new ValidationException($"Validation failed for {fieldName}");
        exception.AddError(fieldName, errorMessage);
        return exception;
    }
}
