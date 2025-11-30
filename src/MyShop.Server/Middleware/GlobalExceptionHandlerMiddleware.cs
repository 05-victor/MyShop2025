using System.Net;
using System.Text.Json;
using MyShop.Server.Exceptions;
using MyShop.Shared.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace MyShop.Server.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and converts them to consistent ApiResponse format
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log the exception with full details
        LogException(exception, context);

        // Prepare error response
        var errorResponse = CreateErrorResponse(exception);

        // Set response headers
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.Code;

        // Serialize and write response
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    private ApiResponse<object> CreateErrorResponse(Exception exception)
    {
        return exception switch
        {
            // Custom application exceptions
            BaseApplicationException appException => CreateApplicationExceptionResponse(appException),

            // Validation exceptions from model binding
            Microsoft.AspNetCore.Http.BadHttpRequestException badRequestEx => 
                ApiResponse<object>.ErrorResponse(
                    "Invalid request format",
                    StatusCodes.Status400BadRequest),

            // Entity Framework exceptions
            DbUpdateException dbUpdateEx => CreateDatabaseExceptionResponse(dbUpdateEx),

            // Unauthorized access
            UnauthorizedAccessException unauthorizedException =>
                ApiResponse<object>.ErrorResponse(
                    "You do not have permission to perform this action",
                    StatusCodes.Status403Forbidden),

            // Argument exceptions
            ArgumentNullException argNullEx =>
                ApiResponse<object>.ErrorResponse(
                    $"Required parameter is missing: {argNullEx.ParamName}",
                    StatusCodes.Status400BadRequest),

            ArgumentException argEx =>
                ApiResponse<object>.ErrorResponse(
                    argEx.Message,
                    StatusCodes.Status400BadRequest),

            // Default fallback for unexpected exceptions
            _ => CreateUnexpectedExceptionResponse(exception)
        };
    }

    private ApiResponse<object> CreateApplicationExceptionResponse(BaseApplicationException exception)
    {
        var response = new ApiResponse<object>
        {
            Code = exception.StatusCode,
            Message = exception.Message,
            Result = null
        };

        // Add validation errors if ValidationException
        if (exception is ValidationException validationException && 
            validationException.ValidationErrors.Any())
        {
            response.Result = new
            {
                Errors = validationException.ValidationErrors
            };
        }

        // Add additional data in development mode
        if (_environment.IsDevelopment() && exception.AdditionalData?.Any() == true)
        {
            response.Result = new
            {
                Errors = (response.Result as dynamic)?.Errors, // only exists if ValidationException
                AdditionalData = exception.AdditionalData,
                ErrorCode = exception.ErrorCode,
                Timestamp = exception.Timestamp
            };
        }

        return response;
    }

    private ApiResponse<object> CreateDatabaseExceptionResponse(DbUpdateException exception)
    {
        _logger.LogError(exception, "Database update error occurred");

        // Check for specific database errors
        var innerException = exception.InnerException?.Message ?? exception.Message;

        if (innerException.Contains("duplicate key") || 
            innerException.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<object>.ErrorResponse(
                "A record with this information already exists",
                StatusCodes.Status409Conflict);
        }

        if (innerException.Contains("FOREIGN KEY constraint", StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<object>.ErrorResponse(
                "This operation would violate data integrity constraints",
                StatusCodes.Status400BadRequest);
        }

        // Generic database error
        return ApiResponse<object>.ErrorResponse(
            _environment.IsDevelopment() 
                ? $"Database error: {innerException}"
                : "A database error occurred. Please try again",
            StatusCodes.Status500InternalServerError);
    }

    private ApiResponse<object> CreateUnexpectedExceptionResponse(Exception exception)
    {
        // Return detailed error in development, generic in production
        var message = _environment.IsDevelopment()
            ? $"An unexpected error occurred: {exception.Message}"
            : "An unexpected error occurred. Please try again later";

        var response = ApiResponse<object>.ServerErrorResponse(message);

        // Add stack trace in development
        if (_environment.IsDevelopment())
        {
            response.Result = new
            {
                ExceptionType = exception.GetType().Name,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException?.Message
            };
        }

        return response;
    }

    private void LogException(Exception exception, HttpContext context)
    {
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var userId = context.User?.Identity?.Name ?? "Anonymous";

        // Different log levels based on exception type
        switch (exception)
        {
            case NotFoundException _:
                _logger.LogWarning(
                    "Resource not found - Method: {Method}, Path: {Path}, User: {User}, Message: {Message}",
                    requestMethod, requestPath, userId, exception.Message);
                break;

            case ValidationException _:
                _logger.LogWarning(
                    "Validation error - Method: {Method}, Path: {Path}, User: {User}, Message: {Message}",
                    requestMethod, requestPath, userId, exception.Message);
                break;

            case BusinessRuleException _:
                _logger.LogWarning(
                    "Business rule violation - Method: {Method}, Path: {Path}, User: {User}, Message: {Message}",
                    requestMethod, requestPath, userId, exception.Message);
                break;

            case AuthenticationException _:
                _logger.LogWarning(
                    "Authentication error - Method: {Method}, Path: {Path}, User: {User}, Message: {Message}",
                    requestMethod, requestPath, userId, exception.Message);
                break;

            case InfrastructureException _:
            case DbUpdateException _:
                _logger.LogError(exception,
                    "Infrastructure error - Method: {Method}, Path: {Path}, User: {User}",
                    requestMethod, requestPath, userId);
                break;

            default:
                _logger.LogError(exception,
                    "Unexpected error - Method: {Method}, Path: {Path}, User: {User}",
                    requestMethod, requestPath, userId);
                break;
        }
    }
}

/// <summary>
/// Extension method to register the global exception handler middleware
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
