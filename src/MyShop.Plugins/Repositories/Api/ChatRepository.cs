using MyShop.Core.Interfaces.Services;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// Real API implementation of IChatService.
/// Integrates with https://agent.lethanhcong.site/chat-bot/chat
/// Note: Role prefix should be added by caller before passing message to this service.
/// </summary>
public class ChatRepository : IChatService
{
    private readonly List<ChatMessage> _history = new();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _userId;
    private readonly string _bearerToken;
    private readonly int _timeoutSeconds;
    private readonly string _baseUrl = "https://agent.lethanhcong.site";

    public ChatRepository(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        // Get all config from appsettings.json
        _apiKey = _configuration["Chatbot:ApiKey"] ?? throw new InvalidOperationException(
            "Chatbot API key not found in appsettings.json. Add 'Chatbot:ApiKey' configuration.");
        
        _userId = _configuration["Chatbot:userId"] ?? throw new InvalidOperationException(
            "Chatbot userId not found in appsettings.json. Add 'Chatbot:userId' configuration.");
        
        _bearerToken = _configuration["Chatbot:Bearer"] ?? throw new InvalidOperationException(
            "Chatbot Bearer token not found in appsettings.json. Add 'Chatbot:Bearer' configuration.");
        
        // Get timeout (default to 180 seconds = 3 minutes if not specified)
        _timeoutSeconds = _configuration.GetValue<int>("Chatbot:TimeoutSeconds", 180);
        System.Diagnostics.Debug.WriteLine($"[Chatbot] Configured timeout: {_timeoutSeconds} seconds");
    }

    /// <inheritdoc/>
    public async Task<ChatResponse> SendMessageAsync(string message)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[Chatbot] ===== SEND MESSAGE =====");
            System.Diagnostics.Debug.WriteLine($"[Chatbot] Message: {message}");
            
            // Store user message in history
            var userMessage = new ChatMessage
            {
                Content = message,
                IsUser = true,
                Timestamp = DateTime.Now
            };
            _history.Add(userMessage);
            
            System.Diagnostics.Debug.WriteLine($"[Chatbot] UserId: {_userId}");
            System.Diagnostics.Debug.WriteLine($"[Chatbot] Bearer: {_bearerToken}");

            // Create HTTP client
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
            System.Diagnostics.Debug.WriteLine($"[Chatbot] HTTP Timeout set to: {_timeoutSeconds} seconds");

            // Build FormData (message should already have role prefix from caller)
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(message), "message");

            // Create request
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat-bot/chat")
            {
                Content = formData
            };

            // Add headers
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("userId", _userId);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
            
            System.Diagnostics.Debug.WriteLine($"[Chatbot] API Endpoint: {_baseUrl}/chat-bot/chat");
            System.Diagnostics.Debug.WriteLine($"[Chatbot] Headers:");
            System.Diagnostics.Debug.WriteLine($"  - x-api-key: {_apiKey?.Substring(0, Math.Min(20, _apiKey.Length))}...");
            System.Diagnostics.Debug.WriteLine($"  - userId: {_userId}");
            System.Diagnostics.Debug.WriteLine($"  - Authorization: Bearer {_bearerToken}");

            // Send request
            System.Diagnostics.Debug.WriteLine($"[Chatbot] Sending request...");
            var response = await httpClient.SendAsync(request);
            System.Diagnostics.Debug.WriteLine($"[Chatbot] Response Status: {response.StatusCode}");

            // Check response
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[Chatbot] ERROR Response: {errorContent}");
                throw new HttpRequestException($"API returned {response.StatusCode}: {errorContent}");
            }

            // Read response text
            var botResponseText = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[Chatbot] Bot Response: {botResponseText}");
            
            // Parse JSON response
            string finalAnswer;
            string? imageUrl = null;
            try
            {
                using var jsonDoc = JsonDocument.Parse(botResponseText);
                var root = jsonDoc.RootElement;
                
                // Check if response has expected structure
                if (root.TryGetProperty("result", out var result) && 
                    result.TryGetProperty("answer", out var answer))
                {
                    finalAnswer = answer.GetString() ?? "No response from bot.";
                    System.Diagnostics.Debug.WriteLine($"[Chatbot] Parsed Answer: {finalAnswer}");
                    
                    // Extract imageUrl if present
                    if (result.TryGetProperty("imageUrl", out var imageUrlProp))
                    {
                        imageUrl = imageUrlProp.GetString();
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            System.Diagnostics.Debug.WriteLine($"[Chatbot] Image URL: {imageUrl}");
                        }
                    }
                }
                else
                {
                    // Fallback: use raw response if structure doesn't match
                    finalAnswer = botResponseText;
                    System.Diagnostics.Debug.WriteLine($"[Chatbot] WARNING: Unexpected JSON structure, using raw response");
                }
            }
            catch (JsonException ex)
            {
                // If JSON parsing fails, use raw response
                System.Diagnostics.Debug.WriteLine($"[Chatbot] JSON Parse Error: {ex.Message}");
                finalAnswer = botResponseText;
            }
            
            System.Diagnostics.Debug.WriteLine($"[Chatbot] ===== END =====");

            // Create AI message
            var aiMessage = new ChatMessage
            {
                Content = finalAnswer,
                IsUser = false,
                Timestamp = DateTime.Now,
                ImageUrl = imageUrl
            };
            _history.Add(aiMessage);

            return ChatResponse.Success(aiMessage);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Chatbot] HttpRequestException: {ex.Message}");
            return ChatResponse.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Chatbot] Timeout: {ex.Message}");
            return ChatResponse.Failure("Request timeout. Please try again.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Chatbot] Exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[Chatbot] StackTrace: {ex.StackTrace}");
            return ChatResponse.Failure($"Error: {ex.Message}");
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
}