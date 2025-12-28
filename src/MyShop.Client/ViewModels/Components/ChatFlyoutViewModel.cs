using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Services;
using ChatMessage = MyShop.Shared.Models.ChatMessage;
using MessageSender = MyShop.Shared.Models.MessageSender;

namespace MyShop.Client.ViewModels.Components;

public partial class ChatFlyoutViewModel : BaseViewModel
{
    private readonly IChatbotService _chatbotService;
    
    [ObservableProperty]
    private string _currentMessage = string.Empty;
    
    [ObservableProperty]
    private bool _isSending;
    
    public ObservableCollection<ChatMessage> Messages { get; } = new();
    
    public bool CanSend => !string.IsNullOrWhiteSpace(CurrentMessage) && !IsSending;
    
    public ChatFlyoutViewModel(IChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
        
        // Welcome message
        Messages.Add(new ChatMessage
        {
            Sender = MessageSender.Bot,
            Content = "Hello! I'm your AI assistant. How can I help you today?"
        });
    }
    
    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage)) return;
        
        var userMessage = CurrentMessage.Trim();
        CurrentMessage = string.Empty;
        
        // Add user message
        Messages.Add(new ChatMessage
        {
            Sender = MessageSender.User,
            Content = userMessage
        });
        
        IsSending = true;
        
        // Call API
        var botResponse = await _chatbotService.SendMessageAsync(userMessage);
        
        // Add bot response
        Messages.Add(new ChatMessage
        {
            Sender = MessageSender.Bot,
            Content = botResponse,
            IsError = botResponse.StartsWith("Error:")
        });
        
        IsSending = false;
    }
    
    private bool CanSendMessage() => CanSend;
    
    partial void OnCurrentMessageChanged(string value)
    {
        SendMessageCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanSend));
    }
    
    partial void OnIsSendingChanged(bool value)
    {
        SendMessageCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanSend));
    }
}
