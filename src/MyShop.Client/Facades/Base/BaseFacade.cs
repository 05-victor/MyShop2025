using MyShop.Core.Common;
using MyShop.Core.Interfaces.Services;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MyShop.Client.Facades.Base;

/// <summary>
/// Base class for all Facades providing common error handling and logging utilities.
/// </summary>
public abstract class BaseFacade
{
    protected readonly IToastService _toastService;

    protected BaseFacade(IToastService toastService)
    {
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    }

    #region Logging Utilities

    /// <summary>
    /// Log debug message with facade name prefix
    /// </summary>
    protected void LogDebug(string message, [CallerMemberName] string? caller = null)
    {
        Debug.WriteLine($"[{GetType().Name}] {caller}: {message}");
    }

    /// <summary>
    /// Log error message with facade name prefix
    /// </summary>
    protected void LogError(string message, Exception? ex = null, [CallerMemberName] string? caller = null)
    {
        var errorMsg = ex != null ? $"{message}: {ex.Message}" : message;
        Debug.WriteLine($"[{GetType().Name}] ERROR {caller}: {errorMsg}");
    }

    #endregion

    #region Error Handling Utilities

    /// <summary>
    /// Execute an async operation with standard error handling.
    /// Shows toast on error and returns failure result.
    /// </summary>
    protected async Task<Result<T>> ExecuteAsync<T>(
        Func<Task<Result<T>>> operation,
        string errorMessage = "Operation failed",
        bool showToastOnError = true,
        [CallerMemberName] string? caller = null)
    {
        try
        {
            var result = await operation();
            
            if (!result.IsSuccess && showToastOnError)
            {
                _toastService.ShowError(result.ErrorMessage ?? errorMessage);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            LogError(errorMessage, ex, caller);
            
            if (showToastOnError)
            {
                _toastService.ShowError($"{errorMessage}: {ex.Message}");
            }
            
            return Result<T>.Failure($"{errorMessage}: {ex.Message}");
        }
    }

    /// <summary>
    /// Execute an async operation with standard error handling (void result).
    /// Shows toast on error and returns failure result.
    /// </summary>
    protected async Task<Result<bool>> ExecuteAsync(
        Func<Task<Result<bool>>> operation,
        string errorMessage = "Operation failed",
        bool showToastOnError = true,
        [CallerMemberName] string? caller = null)
    {
        try
        {
            var result = await operation();
            
            if (!result.IsSuccess && showToastOnError)
            {
                _toastService.ShowError(result.ErrorMessage ?? errorMessage);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            LogError(errorMessage, ex, caller);
            
            if (showToastOnError)
            {
                _toastService.ShowError($"{errorMessage}: {ex.Message}");
            }
            
            return Result<bool>.Failure($"{errorMessage}: {ex.Message}");
        }
    }

    /// <summary>
    /// Execute an async operation that returns data directly (not wrapped in Result).
    /// Wraps the result and handles errors.
    /// </summary>
    protected async Task<Result<T>> ExecuteWithWrapAsync<T>(
        Func<Task<T?>> operation,
        string errorMessage = "Operation failed",
        bool showToastOnError = true,
        [CallerMemberName] string? caller = null) where T : class
    {
        try
        {
            var data = await operation();
            
            if (data == null)
            {
                if (showToastOnError)
                {
                    _toastService.ShowError(errorMessage);
                }
                return Result<T>.Failure(errorMessage);
            }
            
            return Result<T>.Success(data);
        }
        catch (Exception ex)
        {
            LogError(errorMessage, ex, caller);
            
            if (showToastOnError)
            {
                _toastService.ShowError($"{errorMessage}: {ex.Message}");
            }
            
            return Result<T>.Failure($"{errorMessage}: {ex.Message}");
        }
    }

    #endregion

    #region Toast Utilities

    /// <summary>
    /// Show success toast with message
    /// </summary>
    protected void ShowSuccess(string message)
    {
        _toastService.ShowSuccess(message);
    }

    /// <summary>
    /// Show error toast with message
    /// </summary>
    protected void ShowError(string message)
    {
        _toastService.ShowError(message);
    }

    /// <summary>
    /// Show warning toast with message
    /// </summary>
    protected void ShowWarning(string message)
    {
        _toastService.ShowWarning(message);
    }

    /// <summary>
    /// Show info toast with message
    /// </summary>
    protected void ShowInfo(string message)
    {
        _toastService.ShowInfo(message);
    }

    #endregion
}
