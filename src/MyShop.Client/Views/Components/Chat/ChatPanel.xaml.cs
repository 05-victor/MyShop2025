using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MyShop.Client.Views.Components.Chat;

/// <summary>
/// Main chat panel with messages, input, and suggestions.
/// </summary>
public sealed partial class ChatPanel : UserControl
{
    private readonly IChatService? _chatService;

    public ChatPanel()
    {
        this.InitializeComponent();
        _chatService = App.Current.Services?.GetService<IChatService>();
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

        // Show typing indicator
        TypingIndicator.Visibility = Visibility.Visible;
        SendButton.IsEnabled = false;

        try
        {
            // Get AI response
            var response = await _chatService!.SendMessageAsync(message);

            // Hide typing indicator
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
}