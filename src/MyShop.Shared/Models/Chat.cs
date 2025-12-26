namespace MyShop.Shared.Models;

public enum MessageSender
{
    User,
    Bot
}

public class ChatMessage
{
    public MessageSender Sender { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool IsError { get; set; }
}
