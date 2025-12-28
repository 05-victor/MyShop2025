using MyShop.Core.Interfaces.Services;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// Real API implementation of IChatService.
/// TODO: Implement actual AI API integration (OpenAI, Azure OpenAI, etc.)
/// </summary>
public class ChatRepository : IChatService
{
    private readonly List<ChatMessage> _history = new();
    // TODO: Inject HttpClient or AI API client
    // private readonly IHttpClientFactory _httpClientFactory;
    // private readonly string _apiKey;

    public ChatRepository()
    {
        // TODO: Initialize with API configuration
    }

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

            // TODO: Replace with actual API call
            // Example: POST to /api/chat with message payload
            // var response = await _httpClient.PostAsJsonAsync("/api/chat", new { message });
            
            // For now, return a placeholder response
            await Task.Delay(100); // Simulate network delay
            
            var aiMessage = new ChatMessage
            {
                Content = "AI API integration pending. Please configure the chat service endpoint.",
                IsUser = false,
                Timestamp = DateTime.Now
            };
            _history.Add(aiMessage);

            return ChatResponse.Success(aiMessage);
        }
        catch (Exception ex)
        {
            return ChatResponse.Failure($"API Error: {ex.Message}");
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
        "Show me today's deals",
        "I need help with my account",
        "What's your return policy?"
    };
}