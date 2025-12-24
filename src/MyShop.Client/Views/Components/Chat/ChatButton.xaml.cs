using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MyShop.Client.Views.Components.Chat;

/// <summary>
/// Floating chat button for opening the AI chat assistant.
/// </summary>
public sealed partial class ChatButton : UserControl
{
    public event RoutedEventHandler? Click;

    public ChatButton()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
        
        // Subscribe to theme changes
        Services.ThemeManager.ThemeChanged += OnThemeChanged;
    }
    
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyThemeAwareBackground();
    }
    
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Unsubscribe to prevent memory leaks
        Services.ThemeManager.ThemeChanged -= OnThemeChanged;
    }
    
    private void OnThemeChanged(Services.ThemeManager.ThemeType newTheme)
    {
        // Reapply background when theme changes
        DispatcherQueue?.TryEnqueue(() =>
        {
            ApplyThemeAwareBackground();
        });
    }
    
    private void ApplyThemeAwareBackground()
    {
        // Retrieve dedicated ChatButtonBackgroundBrush from theme resources
        // Light: #1A4D8F (Navy), Dark: #60A5FA (Blue-400, 5.2:1 contrast)
        if (Application.Current.Resources.TryGetValue("ChatButtonBackgroundBrush", out var chatButtonBrush)
            && chatButtonBrush is Brush brush)
        {
            FloatingButton.Background = brush;
        }
    }

    /// <summary>
    /// Whether the chat panel is currently open.
    /// </summary>
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(ChatButton),
            new PropertyMetadata(false, OnIsOpenChanged));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Number of unread messages (0 hides badge).
    /// </summary>
    public static readonly DependencyProperty UnreadCountProperty =
        DependencyProperty.Register(
            nameof(UnreadCount),
            typeof(int),
            typeof(ChatButton),
            new PropertyMetadata(0, OnUnreadCountChanged));

    public int UnreadCount
    {
        get => (int)GetValue(UnreadCountProperty);
        set => SetValue(UnreadCountProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ChatButton button)
        {
            var isOpen = (bool)e.NewValue;
            button.ChatIcon.Visibility = isOpen ? Visibility.Collapsed : Visibility.Visible;
            button.CloseIcon.Visibility = isOpen ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnUnreadCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ChatButton button)
        {
            var count = (int)e.NewValue;
            button.NotificationBadge.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void FloatingButton_Click(object sender, RoutedEventArgs e)
    {
        Click?.Invoke(this, e);
    }
}
