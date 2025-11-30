namespace MyShop.Server.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// Maps to 404 Not Found HTTP status
/// </summary>
public class NotFoundException : BaseApplicationException
{
    public NotFoundException(string message, string? entityName = null, object? entityId = null)
        : base(message, "RESOURCE_NOT_FOUND", StatusCodes.Status404NotFound)
    {
        if (entityName != null)
            AddData("EntityName", entityName);
        if (entityId != null)
            AddData("EntityId", entityId);
    }

    /// <summary>
    /// Create a NotFoundException for a specific entity
    /// </summary>
    public static NotFoundException ForEntity(string entityName, object entityId)
        => new($"{entityName} with ID '{entityId}' was not found", entityName, entityId);

    /// <summary>
    /// Create a NotFoundException for a generic resource
    /// </summary>
    public static NotFoundException ForResource(string resourceName)
        => new($"{resourceName} not found", resourceName);
}
