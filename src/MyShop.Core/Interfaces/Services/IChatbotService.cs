namespace MyShop.Core.Interfaces.Services;

public interface IChatbotService
{
    Task<string> SendMessageAsync(string message);
}
