using FluentValidation;
using MediatR;
using MyShop.Core.Common;

namespace MyShop.Client.Common.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior for automatic FluentValidation
/// Validates commands before they reach the handler
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip if no validators registered for this request
        if (!_validators.Any())
        {
            return await next();
        }

        // Run all validators
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // If validation failed, return failure result (assuming TResponse is Result<T>)
        if (failures.Any())
        {
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            
            // Use reflection to create Result<T>.Failure(errorMessage)
            // This works because all our commands return Result<T>
            var responseType = typeof(TResponse);
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var dataType = responseType.GetGenericArguments()[0];
                var failureMethod = typeof(Result<>)
                    .MakeGenericType(dataType)
                    .GetMethod("Failure", new[] { typeof(string), typeof(Exception) });

                if (failureMethod != null)
                {
                    var result = failureMethod.Invoke(null, new object?[] { errorMessage, null });
                    return (TResponse)result!;
                }
            }
        }

        // Validation passed, proceed to handler
        return await next();
    }
}
