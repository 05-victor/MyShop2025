// ============================================================================
// BATCH OPERATION SERVICE
// File: Services/BatchOperationService.cs
// Description: Handles bulk operations on multiple entities
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Client.Services;

/// <summary>
/// Service for performing batch operations on multiple entities.
/// Supports bulk updates, deletes, exports, and status changes.
/// </summary>
public partial class BatchOperationService : ObservableObject, IBatchOperationService
{
    #region Fields

    private readonly IActivityLogService _activityLogService;
    private CancellationTokenSource _currentOperationCts;

    [ObservableProperty]
    private bool _isOperationInProgress;

    [ObservableProperty]
    private int _totalItems;

    [ObservableProperty]
    private int _processedItems;

    [ObservableProperty]
    private int _successCount;

    [ObservableProperty]
    private int _failedCount;

    [ObservableProperty]
    private string _currentOperation;

    [ObservableProperty]
    private double _progress;

    #endregion

    #region Constructor

    public BatchOperationService(IActivityLogService activityLogService = null)
    {
        _activityLogService = activityLogService;
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when an item is processed
    /// </summary>
    public event EventHandler<BatchOperationProgressEventArgs> ProgressChanged;

    /// <summary>
    /// Raised when the batch operation completes
    /// </summary>
    public event EventHandler<BatchOperationCompletedEventArgs> OperationCompleted;

    #endregion

    #region Public Methods

    /// <summary>
    /// Executes a batch operation on a collection of items
    /// </summary>
    /// <typeparam name="T">Type of items</typeparam>
    /// <param name="items">Items to process</param>
    /// <param name="operation">Operation to perform on each item</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>Batch operation result</returns>
    public async Task<BatchOperationResult<T>> ExecuteAsync<T>(
        IEnumerable<T> items,
        Func<T, Task<bool>> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        var itemList = items.ToList();
        
        _currentOperationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        IsOperationInProgress = true;
        TotalItems = itemList.Count;
        ProcessedItems = 0;
        SuccessCount = 0;
        FailedCount = 0;
        CurrentOperation = operationName;
        Progress = 0;

        var result = new BatchOperationResult<T>
        {
            OperationName = operationName,
            TotalItems = itemList.Count,
            StartTime = DateTime.Now
        };

        var successfulItems = new List<T>();
        var failedItems = new List<BatchOperationError<T>>();

        try
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if (_currentOperationCts.Token.IsCancellationRequested)
                {
                    result.WasCancelled = true;
                    break;
                }

                var item = itemList[i];
                ProcessedItems = i + 1;
                Progress = (double)ProcessedItems / TotalItems * 100;

                try
                {
                    var success = await operation(item);
                    
                    if (success)
                    {
                        SuccessCount++;
                        successfulItems.Add(item);
                    }
                    else
                    {
                        FailedCount++;
                        failedItems.Add(new BatchOperationError<T>
                        {
                            Item = item,
                            ErrorMessage = "Operation returned false"
                        });
                    }
                }
                catch (Exception ex)
                {
                    FailedCount++;
                    failedItems.Add(new BatchOperationError<T>
                    {
                        Item = item,
                        ErrorMessage = ex.Message,
                        Exception = ex
                    });
                }

                // Raise progress event
                ProgressChanged?.Invoke(this, new BatchOperationProgressEventArgs
                {
                    CurrentItem = i + 1,
                    TotalItems = itemList.Count,
                    SuccessCount = SuccessCount,
                    FailedCount = FailedCount,
                    Progress = Progress
                });

                // Small delay to allow UI updates
                await Task.Delay(10, _currentOperationCts.Token);
            }
        }
        finally
        {
            IsOperationInProgress = false;
            result.EndTime = DateTime.Now;
            result.SuccessfulItems = successfulItems;
            result.FailedItems = failedItems;
            result.SuccessCount = SuccessCount;
            result.FailedCount = FailedCount;

            // Log the batch operation
            if (_activityLogService != null)
            {
                await _activityLogService.LogActivityAsync(
                    ActivityType.Update,
                    $"Batch {operationName}",
                    $"Processed {TotalItems} items: {SuccessCount} success, {FailedCount} failed",
                    typeof(T).Name,
                    null,
                    new Dictionary<string, object>
                    {
                        ["TotalItems"] = TotalItems,
                        ["SuccessCount"] = SuccessCount,
                        ["FailedCount"] = FailedCount,
                        ["Duration"] = result.Duration.TotalSeconds
                    });
            }

            // Raise completion event
            OperationCompleted?.Invoke(this, new BatchOperationCompletedEventArgs
            {
                OperationName = operationName,
                TotalItems = TotalItems,
                SuccessCount = SuccessCount,
                FailedCount = FailedCount,
                WasCancelled = result.WasCancelled,
                Duration = result.Duration
            });
        }

        return result;
    }

    /// <summary>
    /// Executes batch update on items with a common value
    /// </summary>
    public async Task<BatchOperationResult<T>> BatchUpdateAsync<T, TValue>(
        IEnumerable<T> items,
        Func<T, TValue, Task<bool>> updateAction,
        TValue newValue,
        string propertyName,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            items,
            async item => await updateAction(item, newValue),
            $"Update {propertyName}",
            cancellationToken);
    }

    /// <summary>
    /// Executes batch delete on items
    /// </summary>
    public async Task<BatchOperationResult<T>> BatchDeleteAsync<T>(
        IEnumerable<T> items,
        Func<T, Task<bool>> deleteAction,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            items,
            deleteAction,
            "Delete",
            cancellationToken);
    }

    /// <summary>
    /// Executes batch status change on items
    /// </summary>
    public async Task<BatchOperationResult<T>> BatchChangeStatusAsync<T>(
        IEnumerable<T> items,
        Func<T, string, Task<bool>> statusChangeAction,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            items,
            async item => await statusChangeAction(item, newStatus),
            $"Change Status to {newStatus}",
            cancellationToken);
    }

    /// <summary>
    /// Executes batch category assignment
    /// </summary>
    public async Task<BatchOperationResult<T>> BatchAssignCategoryAsync<T>(
        IEnumerable<T> items,
        Func<T, string, Task<bool>> assignAction,
        string categoryId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            items,
            async item => await assignAction(item, categoryId),
            "Assign Category",
            cancellationToken);
    }

    /// <summary>
    /// Executes batch price update with percentage or fixed amount
    /// </summary>
    public async Task<BatchOperationResult<T>> BatchUpdatePriceAsync<T>(
        IEnumerable<T> items,
        Func<T, decimal, PriceUpdateType, Task<bool>> priceUpdateAction,
        decimal amount,
        PriceUpdateType updateType,
        CancellationToken cancellationToken = default)
    {
        var operationName = updateType switch
        {
            PriceUpdateType.PercentageIncrease => $"Increase Price by {amount}%",
            PriceUpdateType.PercentageDecrease => $"Decrease Price by {amount}%",
            PriceUpdateType.FixedIncrease => $"Increase Price by {amount:C}",
            PriceUpdateType.FixedDecrease => $"Decrease Price by {amount:C}",
            PriceUpdateType.SetFixed => $"Set Price to {amount:C}",
            _ => "Update Price"
        };

        return await ExecuteAsync(
            items,
            async item => await priceUpdateAction(item, amount, updateType),
            operationName,
            cancellationToken);
    }

    /// <summary>
    /// Cancels the current batch operation
    /// </summary>
    public void CancelOperation()
    {
        _currentOperationCts?.Cancel();
    }

    /// <summary>
    /// Creates a batch operation builder for fluent configuration
    /// </summary>
    public BatchOperationBuilder<T> CreateBuilder<T>(IEnumerable<T> items)
    {
        return new BatchOperationBuilder<T>(this, items);
    }

    #endregion
}

#region Interfaces

public interface IBatchOperationService
{
    bool IsOperationInProgress { get; }
    int TotalItems { get; }
    int ProcessedItems { get; }
    int SuccessCount { get; }
    int FailedCount { get; }
    string CurrentOperation { get; }
    double Progress { get; }

    event EventHandler<BatchOperationProgressEventArgs> ProgressChanged;
    event EventHandler<BatchOperationCompletedEventArgs> OperationCompleted;

    Task<BatchOperationResult<T>> ExecuteAsync<T>(
        IEnumerable<T> items,
        Func<T, Task<bool>> operation,
        string operationName,
        CancellationToken cancellationToken = default);

    Task<BatchOperationResult<T>> BatchUpdateAsync<T, TValue>(
        IEnumerable<T> items,
        Func<T, TValue, Task<bool>> updateAction,
        TValue newValue,
        string propertyName,
        CancellationToken cancellationToken = default);

    Task<BatchOperationResult<T>> BatchDeleteAsync<T>(
        IEnumerable<T> items,
        Func<T, Task<bool>> deleteAction,
        CancellationToken cancellationToken = default);

    Task<BatchOperationResult<T>> BatchChangeStatusAsync<T>(
        IEnumerable<T> items,
        Func<T, string, Task<bool>> statusChangeAction,
        string newStatus,
        CancellationToken cancellationToken = default);

    Task<BatchOperationResult<T>> BatchAssignCategoryAsync<T>(
        IEnumerable<T> items,
        Func<T, string, Task<bool>> assignAction,
        string categoryId,
        CancellationToken cancellationToken = default);

    Task<BatchOperationResult<T>> BatchUpdatePriceAsync<T>(
        IEnumerable<T> items,
        Func<T, decimal, PriceUpdateType, Task<bool>> priceUpdateAction,
        decimal amount,
        PriceUpdateType updateType,
        CancellationToken cancellationToken = default);

    void CancelOperation();
    
    BatchOperationBuilder<T> CreateBuilder<T>(IEnumerable<T> items);
}

#endregion

#region Models and Enums

/// <summary>
/// Result of a batch operation
/// </summary>
public class BatchOperationResult<T>
{
    public string OperationName { get; set; }
    public int TotalItems { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public bool WasCancelled { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public List<T> SuccessfulItems { get; set; } = new();
    public List<BatchOperationError<T>> FailedItems { get; set; } = new();
    public bool IsSuccess => FailedCount == 0 && !WasCancelled;
    public bool HasFailures => FailedCount > 0;
}

/// <summary>
/// Error information for a failed item in batch operation
/// </summary>
public class BatchOperationError<T>
{
    public T Item { get; set; }
    public string ErrorMessage { get; set; }
    public Exception Exception { get; set; }
}

/// <summary>
/// Progress event arguments for batch operations
/// </summary>
public class BatchOperationProgressEventArgs : EventArgs
{
    public int CurrentItem { get; set; }
    public int TotalItems { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public double Progress { get; set; }
}

/// <summary>
/// Completion event arguments for batch operations
/// </summary>
public class BatchOperationCompletedEventArgs : EventArgs
{
    public string OperationName { get; set; }
    public int TotalItems { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public bool WasCancelled { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Types of price updates for batch operations
/// </summary>
public enum PriceUpdateType
{
    PercentageIncrease,
    PercentageDecrease,
    FixedIncrease,
    FixedDecrease,
    SetFixed
}

#endregion

#region Fluent Builder

/// <summary>
/// Fluent builder for configuring batch operations
/// </summary>
public class BatchOperationBuilder<T>
{
    private readonly IBatchOperationService _service;
    private readonly IEnumerable<T> _items;
    private Func<T, Task<bool>> _operation;
    private string _operationName;
    private Action<BatchOperationProgressEventArgs> _onProgress;
    private Action<BatchOperationCompletedEventArgs> _onCompleted;
    private CancellationToken _cancellationToken;

    public BatchOperationBuilder(IBatchOperationService service, IEnumerable<T> items)
    {
        _service = service;
        _items = items;
    }

    /// <summary>
    /// Sets the operation to perform on each item
    /// </summary>
    public BatchOperationBuilder<T> WithOperation(Func<T, Task<bool>> operation, string name)
    {
        _operation = operation;
        _operationName = name;
        return this;
    }

    /// <summary>
    /// Sets the progress callback
    /// </summary>
    public BatchOperationBuilder<T> OnProgress(Action<BatchOperationProgressEventArgs> callback)
    {
        _onProgress = callback;
        return this;
    }

    /// <summary>
    /// Sets the completion callback
    /// </summary>
    public BatchOperationBuilder<T> OnCompleted(Action<BatchOperationCompletedEventArgs> callback)
    {
        _onCompleted = callback;
        return this;
    }

    /// <summary>
    /// Sets the cancellation token
    /// </summary>
    public BatchOperationBuilder<T> WithCancellation(CancellationToken token)
    {
        _cancellationToken = token;
        return this;
    }

    /// <summary>
    /// Executes the configured batch operation
    /// </summary>
    public async Task<BatchOperationResult<T>> ExecuteAsync()
    {
        if (_operation == null)
            throw new InvalidOperationException("Operation must be configured using WithOperation");

        if (_onProgress != null)
        {
            _service.ProgressChanged += (s, e) => _onProgress(e);
        }

        if (_onCompleted != null)
        {
            _service.OperationCompleted += (s, e) => _onCompleted(e);
        }

        return await _service.ExecuteAsync(_items, _operation, _operationName, _cancellationToken);
    }
}

#endregion
