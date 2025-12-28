namespace MyShop.Core.Interfaces.Services;

/// <summary>
/// Interface for AI chat assistant service.
/// Supports both mock and real API implementations.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Sends a message and gets a response from the AI assistant.
    /// </summary>
    /// <param name="message">User's message</param>
    /// <returns>AI response message</returns>
    Task<ChatResponse> SendMessageAsync(string message);

    /// <summary>
    /// Gets the chat history for current session.
    /// </summary>
    IReadOnlyList<ChatMessage> GetHistory();

    /// <summary>
    /// Clears the chat history.
    /// </summary>
    void ClearHistory();

    /// <summary>
    /// Gets suggested prompts for the user.
    /// </summary>
    string[] GetSuggestedPrompts();
}

/// <summary>
/// Represents a chat message.
/// </summary>
public class ChatMessage
{
    public string Content { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Response from the chat service.
/// </summary>
public class ChatResponse
{
    public bool IsSuccess { get; set; }
    public ChatMessage? Message { get; set; }
    public string? ErrorMessage { get; set; }

    public static ChatResponse Success(ChatMessage message) => new() { IsSuccess = true, Message = message };
    public static ChatResponse Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}