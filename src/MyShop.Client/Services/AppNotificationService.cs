// ============================================================================
// APP NOTIFICATION SERVICE
// File: Services/AppNotificationService.cs
// Description: Manages in-app and Windows notifications
// ============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Windows.Storage;

namespace MyShop.Client.Services;

/// <summary>
/// Service for managing in-app notifications and Windows toast notifications.
/// Provides a notification center for the application.
/// </summary>
public partial class AppNotificationService : ObservableObject, IAppNotificationService
{
    #region Fields

    private readonly ObservableCollection<AppNotification> _notifications;
    private readonly int _maxNotifications;
    private bool _isInitialized;

    [ObservableProperty]
    private int _unreadCount;

    [ObservableProperty]
    private bool _hasUnreadNotifications;

    #endregion

    #region Constructor

    public AppNotificationService(int maxNotifications = 100)
    {
        _maxNotifications = maxNotifications;
        _notifications = new ObservableCollection<AppNotification>();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets all notifications
    /// </summary>
    public ObservableCollection<AppNotification> Notifications => _notifications;

    /// <summary>
    /// Gets unread notifications
    /// </summary>
    public IEnumerable<AppNotification> UnreadNotifications =>
        _notifications.Where(n => !n.IsRead);

    #endregion

    #region Events

    /// <summary>
    /// Raised when a new notification is received
    /// </summary>
    public event EventHandler<AppNotification> NotificationReceived;

    /// <summary>
    /// Raised when a notification is clicked
    /// </summary>
    public event EventHandler<AppNotification> NotificationClicked;

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the notification service and registers for Windows notifications
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized) return;

        try
        {
            // Register for Windows App Notifications
            var notificationManager = AppNotificationManager.Default;
            notificationManager.NotificationInvoked += OnWindowsNotificationInvoked;
            notificationManager.Register();

            System.Diagnostics.Debug.WriteLine("AppNotificationService: Successfully registered for Windows notifications");
            _isInitialized = true;
        }
        catch (System.Runtime.InteropServices.COMException ex) when (ex.HResult == unchecked((int)0x80004005))
        {
            // E_FAIL: No COM servers registered - expected in packaged mode without proper manifest
            System.Diagnostics.Debug.WriteLine($"AppNotificationService: COM servers not registered (packaged mode), notifications disabled");
            _isInitialized = true; // Continue without notifications
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AppNotificationService: Failed to initialize: {ex.GetType().Name} - {ex.Message}");
            // Continue without Windows notifications
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Cleans up the notification service
    /// </summary>
    public void Shutdown()
    {
        if (!_isInitialized) return;

        try
        {
            AppNotificationManager.Default.Unregister();
        }
        catch
        {
            // Ignore cleanup errors
        }

        _isInitialized = false;
    }

    #endregion

    #region In-App Notifications

    /// <summary>
    /// Shows an in-app notification
    /// </summary>
    public void Show(
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        string actionText = null,
        Action action = null,
        TimeSpan? autoHide = null)
    {
        var notification = new AppNotification
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.Now,
            ActionText = actionText,
            Action = action,
            AutoHideDelay = autoHide
        };

        AddNotification(notification);
        NotificationReceived?.Invoke(this, notification);
    }

    /// <summary>
    /// Shows an info notification
    /// </summary>
    public void ShowInfo(string title, string message, string actionText = null, Action action = null)
    {
        Show(title, message, NotificationType.Info, actionText, action);
    }

    /// <summary>
    /// Shows a success notification
    /// </summary>
    public void ShowSuccess(string title, string message, string actionText = null, Action action = null)
    {
        Show(title, message, NotificationType.Success, actionText, action);
    }

    /// <summary>
    /// Shows a warning notification
    /// </summary>
    public void ShowWarning(string title, string message, string actionText = null, Action action = null)
    {
        Show(title, message, NotificationType.Warning, actionText, action);
    }

    /// <summary>
    /// Shows an error notification
    /// </summary>
    public void ShowError(string title, string message, string actionText = null, Action action = null)
    {
        Show(title, message, NotificationType.Error, actionText, action);
    }

    /// <summary>
    /// Shows a notification for a new order
    /// </summary>
    public void ShowNewOrder(string orderId, string customerName, decimal total)
    {
        Show(
            "Đơn hàng mới",
            $"Khách hàng {customerName} đã đặt đơn hàng #{orderId} - {total:N0}đ",
            NotificationType.Success,
            "Xem chi tiết",
            () => { /* Navigate to order */ });
    }

    /// <summary>
    /// Shows a notification for low stock
    /// </summary>
    public void ShowLowStock(string productName, int currentStock, int threshold)
    {
        Show(
            "Cảnh báo tồn kho thấp",
            $"Sản phẩm '{productName}' còn {currentStock} (ngưỡng: {threshold})",
            NotificationType.Warning,
            "Xem kho",
            () => { /* Navigate to inventory */ });
    }

    /// <summary>
    /// Shows a notification for payment received
    /// </summary>
    public void ShowPaymentReceived(string orderId, decimal amount, string paymentMethod)
    {
        Show(
            "Nhận thanh toán",
            $"Đã nhận {amount:N0}đ cho đơn hàng #{orderId} qua {paymentMethod}",
            NotificationType.Success);
    }

    /// <summary>
    /// Shows a notification for export complete
    /// </summary>
    public void ShowExportComplete(string exportType, string filePath)
    {
        Show(
            "Xuất file hoàn tất",
            $"Đã xuất {exportType} thành công",
            NotificationType.Success,
            "Mở file",
            async () => await Windows.System.Launcher.LaunchUriAsync(new Uri(filePath)));
    }

    /// <summary>
    /// Shows a notification for import complete
    /// </summary>
    public void ShowImportComplete(string importType, int successCount, int failedCount)
    {
        var type = failedCount > 0 ? NotificationType.Warning : NotificationType.Success;
        var message = failedCount > 0
            ? $"Đã nhập {successCount} {importType}, {failedCount} lỗi"
            : $"Đã nhập thành công {successCount} {importType}";

        Show("Nhập dữ liệu hoàn tất", message, type);
    }

    #endregion

    #region Windows Toast Notifications

    /// <summary>
    /// Shows a Windows toast notification
    /// </summary>
    public async Task ShowWindowsNotificationAsync(
        string title,
        string message,
        string imageUri = null,
        Dictionary<string, string> arguments = null)
    {
        if (!_isInitialized) return;

        try
        {
            var builder = new AppNotificationBuilder()
                .AddText(title)
                .AddText(message);

            // Add hero image if provided
            if (!string.IsNullOrEmpty(imageUri))
            {
                builder.SetHeroImage(new Uri(imageUri));
            }

            // Add arguments for handling click
            if (arguments != null)
            {
                foreach (var arg in arguments)
                {
                    builder.AddArgument(arg.Key, arg.Value);
                }
            }

            var notification = builder.BuildNotification();

            await Task.Run(() =>
            {
                AppNotificationManager.Default.Show(notification);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to show Windows notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows a Windows notification with action buttons
    /// </summary>
    public async Task ShowWindowsNotificationWithActionsAsync(
        string title,
        string message,
        params (string content, string argument)[] buttons)
    {
        if (!_isInitialized) return;

        try
        {
            var builder = new AppNotificationBuilder()
                .AddText(title)
                .AddText(message);

            foreach (var (content, argument) in buttons)
            {
                builder.AddButton(new AppNotificationButton(content)
                    .AddArgument("action", argument));
            }

            var notification = builder.BuildNotification();

            await Task.Run(() =>
            {
                AppNotificationManager.Default.Show(notification);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to show Windows notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows a Windows notification with progress bar
    /// </summary>
    public async Task<string> ShowWindowsProgressNotificationAsync(string title, string status)
    {
        if (!_isInitialized) return null;

        try
        {
            var tag = Guid.NewGuid().ToString("N")[..8];

            var builder = new AppNotificationBuilder()
                .AddText(title)
                .AddProgressBar(new AppNotificationProgressBar()
                    .BindStatus()
                    .BindValue())
                .SetTag(tag);

            var notification = builder.BuildNotification();
            notification.Progress = new AppNotificationProgressData(1)
            {
                Status = status,
                Value = 0
            };

            await Task.Run(() =>
            {
                AppNotificationManager.Default.Show(notification);
            });

            return tag;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to show progress notification: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Updates a Windows progress notification
    /// </summary>
    public async Task UpdateWindowsProgressNotificationAsync(string tag, double progress, string status)
    {
        if (!_isInitialized || string.IsNullOrEmpty(tag)) return;

        try
        {
            var progressData = new AppNotificationProgressData(2)
            {
                Status = status,
                Value = progress
            };

            await Task.Run(async () =>
            {
                await AppNotificationManager.Default.UpdateAsync(progressData, tag);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to update progress notification: {ex.Message}");
        }
    }

    #endregion

    #region Notification Management

    /// <summary>
    /// Marks a notification as read
    /// </summary>
    public void MarkAsRead(string notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;
            UpdateUnreadCount();
        }
    }

    /// <summary>
    /// Marks all notifications as read
    /// </summary>
    public void MarkAllAsRead()
    {
        foreach (var notification in _notifications.Where(n => !n.IsRead))
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;
        }
        UpdateUnreadCount();
    }

    /// <summary>
    /// Removes a notification
    /// </summary>
    public void Remove(string notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null)
        {
            _notifications.Remove(notification);
            UpdateUnreadCount();
        }
    }

    /// <summary>
    /// Clears all notifications
    /// </summary>
    public void ClearAll()
    {
        _notifications.Clear();
        UpdateUnreadCount();
    }

    /// <summary>
    /// Clears read notifications
    /// </summary>
    public void ClearRead()
    {
        var readNotifications = _notifications.Where(n => n.IsRead).ToList();
        foreach (var notification in readNotifications)
        {
            _notifications.Remove(notification);
        }
    }

    /// <summary>
    /// Gets notifications by type
    /// </summary>
    public IEnumerable<AppNotification> GetByType(NotificationType type)
    {
        return _notifications.Where(n => n.Type == type);
    }

    /// <summary>
    /// Gets notifications from today
    /// </summary>
    public IEnumerable<AppNotification> GetToday()
    {
        return _notifications.Where(n => n.Timestamp.Date == DateTime.Today);
    }

    #endregion

    #region Private Methods

    private void AddNotification(AppNotification notification)
    {
        // Add at the beginning (newest first)
        _notifications.Insert(0, notification);

        // Trim old notifications
        while (_notifications.Count > _maxNotifications)
        {
            _notifications.RemoveAt(_notifications.Count - 1);
        }

        UpdateUnreadCount();
    }

    private void UpdateUnreadCount()
    {
        UnreadCount = _notifications.Count(n => !n.IsRead);
        HasUnreadNotifications = UnreadCount > 0;
    }

    private void OnWindowsNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        // Handle notification click from Windows
        var arguments = args.Arguments;

        // Find matching in-app notification if exists
        if (arguments.TryGetValue("notificationId", out var notificationId))
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                NotificationClicked?.Invoke(this, notification);
                notification.Action?.Invoke();
            }
        }

        // Handle custom actions
        if (arguments.TryGetValue("action", out var action))
        {
            HandleNotificationAction(action, arguments);
        }
    }

    private void HandleNotificationAction(string action, IDictionary<string, string> arguments)
    {
        // Override this in derived class or use events to handle specific actions
        System.Diagnostics.Debug.WriteLine($"Notification action: {action}");
    }

    #endregion
}

#region Interfaces

public interface IAppNotificationService
{
    ObservableCollection<AppNotification> Notifications { get; }
    IEnumerable<AppNotification> UnreadNotifications { get; }
    int UnreadCount { get; }
    bool HasUnreadNotifications { get; }

    event EventHandler<AppNotification> NotificationReceived;
    event EventHandler<AppNotification> NotificationClicked;

    void Initialize();
    void Shutdown();

    // In-app notifications
    void Show(string title, string message, NotificationType type = NotificationType.Info,
        string actionText = null, Action action = null, TimeSpan? autoHide = null);
    void ShowInfo(string title, string message, string actionText = null, Action action = null);
    void ShowSuccess(string title, string message, string actionText = null, Action action = null);
    void ShowWarning(string title, string message, string actionText = null, Action action = null);
    void ShowError(string title, string message, string actionText = null, Action action = null);

    // Business notifications
    void ShowNewOrder(string orderId, string customerName, decimal total);
    void ShowLowStock(string productName, int currentStock, int threshold);
    void ShowPaymentReceived(string orderId, decimal amount, string paymentMethod);
    void ShowExportComplete(string exportType, string filePath);
    void ShowImportComplete(string importType, int successCount, int failedCount);

    // Windows notifications
    Task ShowWindowsNotificationAsync(string title, string message, string imageUri = null,
        Dictionary<string, string> arguments = null);
    Task ShowWindowsNotificationWithActionsAsync(string title, string message,
        params (string content, string argument)[] buttons);
    Task<string> ShowWindowsProgressNotificationAsync(string title, string status);
    Task UpdateWindowsProgressNotificationAsync(string tag, double progress, string status);

    // Management
    void MarkAsRead(string notificationId);
    void MarkAllAsRead();
    void Remove(string notificationId);
    void ClearAll();
    void ClearRead();
    IEnumerable<AppNotification> GetByType(NotificationType type);
    IEnumerable<AppNotification> GetToday();
}

#endregion

#region Models

/// <summary>
/// Represents an in-app notification
/// </summary>
public class AppNotification : ObservableObject
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public DateTime Timestamp { get; set; }

    private bool _isRead;
    public bool IsRead
    {
        get => _isRead;
        set => SetProperty(ref _isRead, value);
    }

    public DateTime? ReadAt { get; set; }
    public string ActionText { get; set; }
    public Action Action { get; set; }
    public TimeSpan? AutoHideDelay { get; set; }
    public Dictionary<string, object> Data { get; set; }

    /// <summary>
    /// Gets icon for the notification type
    /// </summary>
    public string Icon => Type switch
    {
        NotificationType.Info => "\uE946",      // Info icon
        NotificationType.Success => "\uE73E",    // Checkmark icon
        NotificationType.Warning => "\uE7BA",    // Warning icon
        NotificationType.Error => "\uE783",      // Error icon
        NotificationType.Order => "\uE719",      // Shop icon
        NotificationType.Payment => "\uE8C7",    // Money icon
        NotificationType.Stock => "\uE7B8",      // Package icon
        NotificationType.System => "\uE713",     // Settings icon
        _ => "\uE946"
    };

    /// <summary>
    /// Gets color for the notification type
    /// </summary>
    public string Color => Type switch
    {
        NotificationType.Info => "#0078D4",
        NotificationType.Success => "#107C10",
        NotificationType.Warning => "#FF8C00",
        NotificationType.Error => "#E81123",
        NotificationType.Order => "#8764B8",
        NotificationType.Payment => "#00B294",
        NotificationType.Stock => "#C239B3",
        NotificationType.System => "#767676",
        _ => "#0078D4"
    };

    /// <summary>
    /// Gets time ago display text
    /// </summary>
    public string TimeAgo
    {
        get
        {
            var diff = DateTime.Now - Timestamp;
            if (diff.TotalMinutes < 1) return "Vừa xong";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";
            return Timestamp.ToString("dd/MM/yyyy");
        }
    }
}

/// <summary>
/// Types of notifications
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    Order,
    Payment,
    Stock,
    System
}

#endregion
