using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;

namespace MyShop.Client.Views.Components.Chat;

/// <summary>
/// Message bubble component for chat UI.
/// </summary>
public sealed partial class MessageBubble : UserControl
{
    public MessageBubble()
    {
        this.InitializeComponent();
        UpdateStyle();
    }

    /// <summary>
    /// The message content.
    /// </summary>
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(MessageBubble),
            new PropertyMetadata(string.Empty));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Whether this is a user message (true) or AI message (false).
    /// </summary>
    public static readonly DependencyProperty IsUserProperty =
        DependencyProperty.Register(
            nameof(IsUser),
            typeof(bool),
            typeof(MessageBubble),
            new PropertyMetadata(false, OnIsUserChanged));

    public bool IsUser
    {
        get => (bool)GetValue(IsUserProperty);
        set => SetValue(IsUserProperty, value);
    }

    /// <summary>
    /// Message timestamp.
    /// </summary>
    public static readonly DependencyProperty TimestampProperty =
        DependencyProperty.Register(
            nameof(Timestamp),
            typeof(DateTime),
            typeof(MessageBubble),
            new PropertyMetadata(DateTime.Now, OnTimestampChanged));

    public DateTime Timestamp
    {
        get => (DateTime)GetValue(TimestampProperty);
        set => SetValue(TimestampProperty, value);
    }

    private static void OnIsUserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MessageBubble bubble)
            bubble.UpdateStyle();
    }

    private static void OnTimestampChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MessageBubble bubble && e.NewValue is DateTime timestamp)
        {
            bubble.TimestampText.Text = timestamp.ToString("HH:mm");
        }
    }

    private void UpdateStyle()
    {
        if (IsUser)
        {
            // User message - right aligned, primary color
            BubbleBorder.HorizontalAlignment = HorizontalAlignment.Right;
            BubbleBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 26, 77, 143)); // Primary
            BubbleBorder.CornerRadius = new CornerRadius(12, 12, 4, 12);
            
            foreach (var child in ((StackPanel)BubbleBorder.Child).Children)
            {
                if (child is TextBlock textBlock)
                    textBlock.Foreground = new SolidColorBrush(Colors.White);
            }
        }
        else
        {
            // AI message - left aligned, gray background
            BubbleBorder.HorizontalAlignment = HorizontalAlignment.Left;
            BubbleBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 243, 244, 246)); // Gray100
            BubbleBorder.CornerRadius = new CornerRadius(12, 12, 12, 4);
            
            foreach (var child in ((StackPanel)BubbleBorder.Child).Children)
            {
                if (child is TextBlock textBlock)
                    textBlock.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 17, 24, 39)); // Gray900
            }
        }
    }
}
