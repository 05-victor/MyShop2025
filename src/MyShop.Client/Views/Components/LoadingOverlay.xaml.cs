using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Components;

/// <summary>
/// Reusable loading overlay component with ProgressRing and message.
/// Usage:
/// <components:LoadingOverlay IsLoading="{x:Bind ViewModel.IsLoading, Mode=OneWay}"
///                            Message="{x:Bind ViewModel.LoadingMessage, Mode=OneWay}"/>
/// </summary>
public sealed partial class LoadingOverlay : UserControl
{
    public LoadingOverlay()
    {
        InitializeComponent();
        this.Loaded += LoadingOverlay_Loaded;
    }

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(LoadingOverlay),
            new PropertyMetadata(false, OnIsLoadingChanged));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(LoadingOverlay),
            new PropertyMetadata(string.Empty));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty ShowSubMessageProperty =
        DependencyProperty.Register(
            nameof(ShowSubMessage),
            typeof(Visibility),
            typeof(LoadingOverlay),
            new PropertyMetadata(Visibility.Collapsed));

    public Visibility ShowSubMessage
    {
        get => (Visibility)GetValue(ShowSubMessageProperty);
        set => SetValue(ShowSubMessageProperty, value);
    }

    private void LoadingOverlay_Loaded(object sender, RoutedEventArgs e)
    {
        if (IsLoading)
        {
            FadeInAnimation.Begin();
        }
    }

    private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingOverlay overlay && e.NewValue is bool isLoading && isLoading)
        {
            overlay.FadeInAnimation.Begin();
        }
    }
}
