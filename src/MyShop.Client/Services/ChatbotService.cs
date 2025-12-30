using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models.Enums;

namespace MyShop.Client.Services;

public class ChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly ICurrentUserService _currentUserService;
    private readonly string? _apiKey;
    private readonly string _baseUrl = "https://agent.lethanhcong.site";
    private readonly bool _isConfigured;
    
    public ChatbotService(
        IHttpClientFactory httpClientFactory, 
        ICurrentUserService currentUserService,
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _currentUserService = currentUserService;
        
        // API Key from appsettings.json
        _apiKey = configuration["Chatbot:ApiKey"];
        _isConfigured = !string.IsNullOrWhiteSpace(_apiKey) && 
                        _apiKey != "YOUR_API_KEY_HERE";
    }
    
    public async Task<string> SendMessageAsync(string message)
    {
        if (!_isConfigured)
        {
            return "Error: Chatbot is not configured. Please contact administrator.";
        }
        
        try
        {
            var userId = _currentUserService.CurrentUser?.Id.ToString() ?? string.Empty;
            
            // Format message with role prefix
            var rolePrefix = GetRolePrefix();
            var formattedMessage = $"[{rolePrefix}]: {message}";
            
            var formData = new MultipartFormDataContent
            {
                { new StringContent(formattedMessage), "message" }
            };
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat-bot/chat")
            {
                Content = formData
            };
            
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userId);
            
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                return $"Error: Unable to connect to chatbot (Status: {response.StatusCode})";
            }
            
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException)
        {
            return "Error: Network connection failed. Please check your internet.";
        }
        catch (TaskCanceledException)
        {
            return "Error: Request timeout. Please try again.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Get role prefix based on current user's role.
    /// </summary>
    private string GetRolePrefix()
    {
        try
        {
            var currentUser = _currentUserService.CurrentUser;
            if (currentUser == null)
                return "USER";
            
            var primaryRole = currentUser.GetPrimaryRole();
            return primaryRole switch
            {
                UserRole.Admin => "ADMIN",
                UserRole.SalesAgent => "SALER",
                UserRole.Customer => "USER",
                _ => "USER"
            };
        }
        catch
        {
            // Fallback to USER if any error occurs
            return "USER";
        }
    }
}
