using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using MyShop.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Shared.Models.Enums;
using MyShop.Client.Services;

namespace MyShop.Client.Views.Components.Chat;

/// <summary>
/// Main chat panel with messages, input, and suggestions.
/// </summary>
public sealed partial class ChatPanel : UserControl
{
    private readonly IChatService? _chatService;
    private readonly ICurrentUserService? _currentUserService;

    public ChatPanel()
    {
        this.InitializeComponent();
        _chatService = App.Current.Services?.GetService<IChatService>();
        _currentUserService = App.Current.Services?.GetService<ICurrentUserService>();
        LoadSuggestions();
    }

    private void LoadSuggestions()
    {
        SuggestionsPanel.Children.Clear();
        var prompts = _chatService?.GetSuggestedPrompts() ?? new[] { "How can I help you?" };
        
        // Get the style from resources
        var buttonStyle = Resources["SuggestionButtonStyle"] as Style;
        
        foreach (var prompt in prompts)
        {
            var button = new Button
            {
                Content = prompt,
                Style = buttonStyle // Apply XAML-defined style
            };
            
            button.Click += SuggestionButton_Click;
            SuggestionsPanel.Children.Add(button);
        }
    }

    private void SuggestionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Content is string prompt)
        {
            InputTextBox.Text = prompt;
            SendMessage();
        }
    }

    private async void SendMessage()
    {
        var message = InputTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(message))
            return;

        // Clear input
        InputTextBox.Text = string.Empty;
        
        // Hide suggestions after first message
        SuggestionsArea.Visibility = Visibility.Collapsed;

        // Add user message
        var userBubble = new MessageBubble
        {
            Message = message,
            IsUser = true,
            Timestamp = System.DateTime.Now
        };
        MessagesPanel.Children.Add(userBubble);
        ScrollToBottom();

        // Show typing indicator with animation
        TypingIndicator.Visibility = Visibility.Visible;
        StartTypingAnimation();
        SendButton.IsEnabled = false;

        try
        {
            // Add role prefix to message before sending to API
            var rolePrefix = GetRolePrefix();
            var formattedMessage = $"[{rolePrefix}]: {message}";
            
            System.Diagnostics.Debug.WriteLine($"[ChatPanel] Sending message with prefix [{rolePrefix}]");

            // Get AI response
            var response = await _chatService!.SendMessageAsync(formattedMessage);

            // Hide typing indicator and stop animation
            StopTypingAnimation();
            TypingIndicator.Visibility = Visibility.Collapsed;

            if (response.IsSuccess && response.Message != null)
            {
                // Add AI message
                var aiBubble = new MessageBubble
                {
                    Message = response.Message.Content,
                    IsUser = false,
                    Timestamp = response.Message.Timestamp,
                    ImageUrl = response.Message.ImageUrl
                };
                MessagesPanel.Children.Add(aiBubble);
                ScrollToBottom();
            }
            else
            {
                throw new Exception(response.ErrorMessage ?? "Unknown error");
            }
        }
        catch
        {
            StopTypingAnimation();
            TypingIndicator.Visibility = Visibility.Collapsed;
            
            // Show error message
            var errorBubble = new MessageBubble
            {
                Message = "Sorry, I couldn't process that. Please try again.",
                IsUser = false,
                Timestamp = System.DateTime.Now
            };
            MessagesPanel.Children.Add(errorBubble);
        }
        finally
        {
            SendButton.IsEnabled = true;
        }
    }

    private void ScrollToBottom()
    {
        MessagesScrollViewer.UpdateLayout();
        MessagesScrollViewer.ChangeView(null, MessagesScrollViewer.ScrollableHeight, null);
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        SendMessage();
    }

    private void InputTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            SendMessage();
            e.Handled = true;
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _chatService?.ClearHistory();
        
        // Keep only welcome message
        while (MessagesPanel.Children.Count > 1)
        {
            MessagesPanel.Children.RemoveAt(MessagesPanel.Children.Count - 1);
        }
        
        // Show suggestions again
        SuggestionsArea.Visibility = Visibility.Visible;
        LoadSuggestions();
    }

    /// <summary>
    /// Start the typing animation for the indicator dots.
    /// </summary>
    private void StartTypingAnimation()
    {
        if (Resources["TypingAnimation"] is Storyboard animation)
        {
            animation.Begin();
        }
    }

    /// <summary>
    /// Stop the typing animation.
    /// </summary>
    private void StopTypingAnimation()
    {
        if (Resources["TypingAnimation"] is Storyboard animation)
        {
            animation.Stop();
        }
    }

    /// <summary>
    /// Get role prefix based on current user's role.
    /// Follows format from 00-SYSTEM-PROMPT.md:
    /// [USER]: message → Customer
    /// [SALER]: message → Sales Agent
    /// [ADMIN]: message → Admin
    /// </summary>
    private string GetRolePrefix()
    {
        try
        {
            var currentUser = _currentUserService?.CurrentUser;
            System.Diagnostics.Debug.WriteLine($"[ChatPanel.GetRolePrefix] CurrentUserService null? {_currentUserService == null}");
            System.Diagnostics.Debug.WriteLine($"[ChatPanel.GetRolePrefix] CurrentUser null? {currentUser == null}");
            
            if (currentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("[ChatPanel.GetRolePrefix] CurrentUser is null, returning USER");
                return "USER";
            }
            
            System.Diagnostics.Debug.WriteLine($"[ChatPanel.GetRolePrefix] CurrentUser.Username: {currentUser.Username}");
            System.Diagnostics.Debug.WriteLine($"[ChatPanel.GetRolePrefix] CurrentUser.Roles count: {currentUser.Roles?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"[ChatPanel.GetRolePrefix] CurrentUser.Roles: {string.Join(", ", currentUser.Roles ?? new List<UserRole>())}");
            
            var primaryRole = currentUser.GetPrimaryRole();
            System.Diagnostics.Debug.WriteLine($"[ChatPanel.GetRolePrefix] PrimaryRole: {primaryRole}");
            
            var prefix = primaryRole switch
            {
                UserRole.Admin => "ADMIN",
                UserRole.SalesAgent => "SALER",
                UserRole.Customer => "USER",
                _ => "USER"
            };
            
            System.Diagnostics.Debug.WriteLine($"[ChatPanel.GetRolePrefix] Returning prefix: {prefix}");
            return prefix;
        }
        catch (Exception ex)
        {
            // Fallback to USER if any error occurs
            System.Diagnostics.Debug.WriteLine($"[ChatPanel] Error getting role prefix: {ex.Message}, defaulting to USER");
            return "USER";
        }
    }
}