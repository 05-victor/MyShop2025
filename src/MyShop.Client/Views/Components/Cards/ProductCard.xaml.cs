using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using MyShop.Client.ViewModels.Shared;
using System.Windows.Input;

namespace MyShop.Client.Views.Components.Cards;

public sealed partial class ProductCard : UserControl
{
    private readonly Storyboard _liftUpAnimation;
    private readonly Storyboard _liftDownAnimation;

    public ProductCard()
    {
        this.InitializeComponent();
        
        // Create lift animations
        _liftUpAnimation = CreateLiftAnimation(-4);
        _liftDownAnimation = CreateLiftAnimation(0);
    }

    #region Dependency Properties

    public ProductCardViewModel Product
    {
        get => (ProductCardViewModel)GetValue(ProductProperty);
        set => SetValue(ProductProperty, value);
    }

    public static readonly DependencyProperty ProductProperty =
        DependencyProperty.Register(
            nameof(Product),
            typeof(ProductCardViewModel),
            typeof(ProductCard),
            new PropertyMetadata(null));

    public ICommand? AddToCartCommand
    {
        get => (ICommand?)GetValue(AddToCartCommandProperty);
        set => SetValue(AddToCartCommandProperty, value);
    }

    public static readonly DependencyProperty AddToCartCommandProperty =
        DependencyProperty.Register(
            nameof(AddToCartCommand),
            typeof(ICommand),
            typeof(ProductCard),
            new PropertyMetadata(null));

    public ICommand? ViewDetailsCommand
    {
        get => (ICommand?)GetValue(ViewDetailsCommandProperty);
        set => SetValue(ViewDetailsCommandProperty, value);
    }

    public static readonly DependencyProperty ViewDetailsCommandProperty =
        DependencyProperty.Register(
            nameof(ViewDetailsCommand),
            typeof(ICommand),
            typeof(ProductCard),
            new PropertyMetadata(null));

    public ICommand? VerifyEmailCommand
    {
        get => (ICommand?)GetValue(VerifyEmailCommandProperty);
        set => SetValue(VerifyEmailCommandProperty, value);
    }

    public static readonly DependencyProperty VerifyEmailCommandProperty =
        DependencyProperty.Register(
            nameof(VerifyEmailCommand),
            typeof(ICommand),
            typeof(ProductCard),
            new PropertyMetadata(null));

    public bool CanAddToCart
    {
        get => (bool)GetValue(CanAddToCartProperty);
        set => SetValue(CanAddToCartProperty, value);
    }

    public static readonly DependencyProperty CanAddToCartProperty =
        DependencyProperty.Register(
            nameof(CanAddToCart),
            typeof(bool),
            typeof(ProductCard),
            new PropertyMetadata(true));

    public bool ShowEmailVerification
    {
        get => (bool)GetValue(ShowEmailVerificationProperty);
        set => SetValue(ShowEmailVerificationProperty, value);
    }

    public static readonly DependencyProperty ShowEmailVerificationProperty =
        DependencyProperty.Register(
            nameof(ShowEmailVerification),
            typeof(bool),
            typeof(ProductCard),
            new PropertyMetadata(false));

    #endregion

    #region Event Handlers

    private void CardBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _liftUpAnimation.Begin();
    }

    private void CardBorder_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        _liftDownAnimation.Begin();
    }

    #endregion

    #region Helper Methods

    private Storyboard CreateLiftAnimation(double targetY)
    {
        var storyboard = new Storyboard();
        var animation = new DoubleAnimation
        {
            To = targetY,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(animation, CardBorder);
        Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(TranslateTransform.Y)");
        storyboard.Children.Add(animation);
        
        return storyboard;
    }

    #endregion
}
