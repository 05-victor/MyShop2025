using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Components;

namespace MyShop.Client.Views.Components.Chat;

public sealed partial class ChatFlyout : UserControl
{
    public ChatFlyoutViewModel ViewModel { get; }
    
    public ChatFlyout()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ChatFlyoutViewModel>();
        
        // Auto-scroll to bottom when new message
        ViewModel.Messages.CollectionChanged += (s, e) =>
        {
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                MessageScrollViewer.ChangeView(null, MessageScrollViewer.ScrollableHeight, null, false);
            });
        };
    }
}
