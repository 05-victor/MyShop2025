# Global Exception Handling System

## Overview

This project implements a centralized exception handling system that automatically catches all exceptions and converts them into consistent `ApiResponse<T>` format. This eliminates the need for try-catch blocks in controllers and provides meaningful error messages to clients.

## Architecture

### Exception Hierarchy

```
BaseApplicationException (abstract)
??? NotFoundException (404)
??? ValidationException (400)
??? BusinessRuleException (400/409)
??? AuthenticationException (401/403)
??? InfrastructureException (500/503)
```

### Components

1. **Custom Exception Classes** (`src/MyShop.Server/Exceptions/`)
   - `BaseApplicationException`: Base class for all custom exceptions
   - `NotFoundException`: Resource not found (404)
   - `ValidationException`: Validation errors (400)
   - `BusinessRuleException`: Business logic violations (400/409)
   - `AuthenticationException`: Auth/authz errors (401/403)
   - `InfrastructureException`: Database/service errors (500/503)

2. **Global Exception Handler Middleware** (`src/MyShop.Server/Middleware/`)
   - `GlobalExceptionHandlerMiddleware`: Catches all unhandled exceptions
   - Converts exceptions to `ApiResponse<object>` format
   - Logs exceptions with appropriate log levels
   - Returns detailed errors in development, sanitized in production

3. **Service Layer Updates**
   - Services throw meaningful custom exceptions
   - No try-catch needed in controllers
   - Infrastructure errors wrapped in `InfrastructureException`

## Usage

### In Services

```csharp
// Not Found
throw NotFoundException.ForEntity("Product", productId);
throw NotFoundException.ForResource("Configuration");

// Validation
throw ValidationException.ForField("Email", "Email is required");
var validationEx = new ValidationException("Multiple errors");
validationEx.AddError("Name", "Name is required");
validationEx.AddError("Price", "Price must be positive");
throw validationEx;

// Business Rules
throw BusinessRuleException.InvalidOperation("Cannot delete category with products");
throw BusinessRuleException.Conflict("Product SKU already exists");

// Authentication
throw AuthenticationException.InvalidCredentials();
throw AuthenticationException.Forbidden("admin dashboard");
throw AuthenticationException.ExpiredToken();

// Infrastructure
throw InfrastructureException.DatabaseError("Failed to save changes", exception);
throw InfrastructureException.ServiceUnavailable("Payment Gateway");
```

### Exception Flow

1. **Service Layer**: Throws custom exception
2. **Controller**: Propagates exception (no try-catch needed)
3. **Middleware**: Catches exception globally
4. **Response**: Converts to `ApiResponse<object>` with appropriate status code

### Response Format

#### Success Response
```json
{
  "code": 200,
  "message": "Product created successfully",
  "result": { "id": "...", "name": "..." }
}
```

#### Error Response (NotFoundException)
```json
{
  "code": 404,
  "message": "Product with ID '123' was not found",
  "result": null
}
```

#### Error Response (ValidationException)
```json
{
  "code": 400,
  "message": "One or more validation errors occurred",
  "result": {
    "errors": {
      "name": ["Name is required"],
      "price": ["Price must be positive", "Price cannot exceed 10000"]
    }
  }
}
```

#### Error Response (Development Mode)
```json
{
  "code": 500,
  "message": "An unexpected error occurred: Division by zero",
  "result": {
    "exceptionType": "DivideByZeroException",
    "stackTrace": "   at ...",
    "innerException": "..."
  }
}
```

## HTTP Status Codes

| Exception Type | Default Status | Alternative |
|---------------|---------------|-------------|
| NotFoundException | 404 Not Found | - |
| ValidationException | 400 Bad Request | - |
| BusinessRuleException | 400 Bad Request | 409 Conflict |
| AuthenticationException | 401 Unauthorized | 403 Forbidden |
| InfrastructureException | 500 Internal Server Error | 503 Service Unavailable |

## Logging

The middleware logs exceptions with different levels:

- **Warning**: NotFoundException, ValidationException, BusinessRuleException, AuthenticationException
- **Error**: InfrastructureException, DbUpdateException, unexpected exceptions

Log format includes:
- HTTP method and path
- User identity (username or "Anonymous")
- Exception message
- Full stack trace (for Error level)

## Best Practices

### DO ?
```csharp
// Throw specific exceptions
if (product == null)
    throw NotFoundException.ForEntity("Product", id);

// Use static factory methods
throw AuthenticationException.InvalidCredentials();

// Let middleware handle exceptions (no try-catch in controllers)
public async Task<ActionResult> GetProduct(Guid id)
{
    var product = await _productService.GetByIdAsync(id);
    return Ok(ApiResponse<Product>.SuccessResponse(product));
}

// Wrap infrastructure errors
try
{
    await _repository.SaveAsync();
}
catch (Exception ex) when (ex is not BaseApplicationException)
{
    throw InfrastructureException.DatabaseError("Save failed", ex);
}
```

### DON'T ?
```csharp
// Don't return null for "not found"
public async Task<Product?> GetById(Guid id)
{
    return await _repository.FindAsync(id); // ?
}

// Don't use try-catch in controllers
[HttpGet]
public async Task<ActionResult> Get()
{
    try // ? Not needed - middleware handles this
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.Message);
    }
}

// Don't throw generic exceptions
throw new Exception("Something went wrong"); // ?
```

## Testing

### Unit Test Example
```csharp
[Fact]
public async Task CreateProduct_WithInvalidCategory_ThrowsNotFoundException()
{
    // Arrange
    var request = new CreateProductRequest { CategoryId = Guid.NewGuid() };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<NotFoundException>(
        () => _productService.CreateAsync(request)
    );
    
    Assert.Equal(404, exception.StatusCode);
    Assert.Contains("Category", exception.Message);
}
```

### Integration Test Example
```csharp
[Fact]
public async Task GetProduct_WithInvalidId_Returns404()
{
    // Arrange
    var invalidId = Guid.NewGuid();

    // Act
    var response = await _client.GetAsync($"/api/v1/products/{invalidId}");

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    var content = await response.Content.ReadAsStringAsync();
    var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content);
    Assert.Equal(404, apiResponse.Code);
    Assert.Contains("not found", apiResponse.Message.ToLower());
}
```

## Migration Guide

### Before (Old Code)
```csharp
public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
{
    var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
    if (category is null)
    {
        throw new KeyNotFoundException("Category not found");
    }
    // ...
}
```

### After (New Code)
```csharp
public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
{
    var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
    if (category is null)
    {
        throw NotFoundException.ForEntity("Category", request.CategoryId);
    }
    // ...
}
```

## Configuration

The middleware is registered in `Program.cs`:

```csharp
// ? IMPORTANT: Add FIRST (before other middleware)
app.UseGlobalExceptionHandler();
```

## Environment-Specific Behavior

### Development
- Returns detailed error information
- Includes exception type, stack trace, inner exception
- Includes additional data from custom exceptions

### Production
- Returns sanitized error messages
- Hides implementation details
- Logs full error details server-side

## Benefits

1. **Consistency**: All errors follow the same `ApiResponse` format
2. **Maintainability**: Single place to update error handling logic
3. **Clean Controllers**: No try-catch clutter
4. **Better Logging**: Centralized, structured logging
5. **Client-Friendly**: Meaningful error messages with proper HTTP status codes
6. **Type Safety**: Strongly-typed custom exceptions
7. **Debugging**: Detailed errors in development, safe in production

## Future Enhancements

- [ ] Add exception filters for specific scenarios
- [ ] Implement retry logic for transient failures
- [ ] Add correlation IDs for distributed tracing
- [ ] Create custom exception for rate limiting (429)
- [ ] Add localization support for error messages
- [ ] Implement circuit breaker pattern for external services
