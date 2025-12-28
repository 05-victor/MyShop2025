using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Client.ViewModels.Shared;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyShop.Client.Views.Components.Cards;

public sealed partial class GroupCard : UserControl
{
    public GroupCard()
    {
        this.InitializeComponent();
    }

    #region Dependency Properties

    public string GroupName
    {
        get => (string)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    public static readonly DependencyProperty GroupNameProperty =
        DependencyProperty.Register(
            nameof(GroupName),
            typeof(string),
            typeof(GroupCard),
            new PropertyMetadata(string.Empty));

    public ObservableCollection<CheckoutItem> Items
    {
        get => (ObservableCollection<CheckoutItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<CheckoutItem>),
            typeof(GroupCard),
            new PropertyMetadata(null));

    public string Subtotal
    {
        get => (string)GetValue(SubtotalProperty);
        set => SetValue(SubtotalProperty, value);
    }

    public static readonly DependencyProperty SubtotalProperty =
        DependencyProperty.Register(
            nameof(Subtotal),
            typeof(string),
            typeof(GroupCard),
            new PropertyMetadata(string.Empty));

    // Task F: Commands for cart interactions
    public ICommand IncreaseQuantityCommand
    {
        get => (ICommand)GetValue(IncreaseQuantityCommandProperty);
        set => SetValue(IncreaseQuantityCommandProperty, value);
    }

    public static readonly DependencyProperty IncreaseQuantityCommandProperty =
        DependencyProperty.Register(
            nameof(IncreaseQuantityCommand),
            typeof(ICommand),
            typeof(GroupCard),
            new PropertyMetadata(null));

    public ICommand DecreaseQuantityCommand
    {
        get => (ICommand)GetValue(DecreaseQuantityCommandProperty);
        set => SetValue(DecreaseQuantityCommandProperty, value);
    }

    public static readonly DependencyProperty DecreaseQuantityCommandProperty =
        DependencyProperty.Register(
            nameof(DecreaseQuantityCommand),
            typeof(ICommand),
            typeof(GroupCard),
            new PropertyMetadata(null));

    public ICommand RemoveItemCommand
    {
        get => (ICommand)GetValue(RemoveItemCommandProperty);
        set => SetValue(RemoveItemCommandProperty, value);
    }

    public static readonly DependencyProperty RemoveItemCommandProperty =
        DependencyProperty.Register(
            nameof(RemoveItemCommand),
            typeof(ICommand),
            typeof(GroupCard),
            new PropertyMetadata(null));

    #endregion
}
