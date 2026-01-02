using MyShop.Core.Interfaces.Services;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation of IChatService for development/testing.
/// Returns predefined responses with simulated typing delay.
/// Note: Role prefix should be added by caller before passing message to this service.
/// </summary>
public class MockChatRepository : IChatService
{
    private readonly Random _random = new();
    private readonly List<ChatMessage> _history = new();

    private static readonly Dictionary<string, string[]> Responses = new()
    {
        { "hello", new[] { "Hello! How can I help you today?", "Hi there! What can I assist you with?", "Hey! Ready to help. What do you need?" } },
        { "help", new[] { 
            "I can help you with:\n• Product questions\n• Order tracking\n• Account settings\n• General inquiries\n\nJust ask away!", 
            "Sure! I'm here to assist with:\n• Products\n• Orders\n• Settings\n\nWhat would you like to know?" 
        } },
        { "order", new[] { "To check your order status:\n1. Go to Orders in the sidebar\n2. Find your order\n3. Click for details\n\nWould you like me to guide you there?", "I can help with order inquiries.\nWhat's your order number?" } },
        { "product", new[] { 
            "Looking for a specific product?\n\nTry these options:\n• Use the search bar\n• Browse by category\n• Check featured items\n\nCan I help you find something specific?", 
            "We have a great selection!\nWhat kind of product are you looking for?" 
        } },
        { "price", new[] { "Our prices are competitive!\n\nIs there a specific product you'd like to know the price for?", "I can help with pricing info.\nWhich product are you interested in?" } },
        { "thanks", new[] { "You're welcome!\nIs there anything else I can help with?", "Happy to help!\nLet me know if you need anything else.", "Anytime!\nFeel free to ask if you have more questions." } },
        { "bye", new[] { "Goodbye! Have a great day!", "See you later!\nDon't hesitate to come back if you need help.", "Bye! Thanks for chatting!" } },
    };

    private static readonly string[] DefaultResponses = new[]
    {
        "I understand. Could you tell me more about what you're looking for?",
        "That's a great question! Let me help you with that.",
        "I'm here to assist. Could you provide more details?",
        "Thanks for reaching out! How can I make your experience better?"
    };

    /// <inheritdoc/>
    public async Task<ChatResponse> SendMessageAsync(string message)
    {
        try
        {
            // Store user message
            var userMessage = new ChatMessage
            {
                Content = message,
                IsUser = true,
                Timestamp = DateTime.Now
            };
            _history.Add(userMessage);

            // Simulate typing delay (500-1500ms)
            await Task.Delay(_random.Next(500, 1500));

            // Generate response
            var responseText = GenerateResponse(message);
            var aiMessage = new ChatMessage
            {
                Content = responseText,
                IsUser = false,
                Timestamp = DateTime.Now
            };
            _history.Add(aiMessage);

            return ChatResponse.Success(aiMessage);
        }
        catch (Exception ex)
        {
            return ChatResponse.Failure($"Failed to process message: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<ChatMessage> GetHistory() => _history.AsReadOnly();

    /// <inheritdoc/>
    public void ClearHistory() => _history.Clear();

    /// <inheritdoc/>
    public string[] GetSuggestedPrompts() => new[]
    {
        "How do I track my order?",
        "Show me available products",
        "I need help with my account",
        "What are the payment options?"
    };

    private string GenerateResponse(string message)
    {
        var lowerMessage = message.ToLower();

        // Check for keyword matches
        foreach (var (keyword, responses) in Responses)
        {
            if (lowerMessage.Contains(keyword))
            {
                return responses[_random.Next(responses.Length)];
            }
        }

        // Return random default response
        return DefaultResponses[_random.Next(DefaultResponses.Length)];
    }
}