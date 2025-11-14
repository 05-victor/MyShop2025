using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Client.Views.Components.Badges;

/// <summary>
/// Stock badge component with automatic color based on quantity.
/// Usage:
/// <badges:StockBadge Stock="{x:Bind Product.Quantity, Mode=OneWay}" 
///                    LowStockThreshold="10"/>
/// </summary>
public sealed partial class StockBadge : UserControl
{
    public StockBadge()
    {
        InitializeComponent();
        UpdateStockDisplay();
    }

    public static readonly DependencyProperty StockProperty =
        DependencyProperty.Register(
            nameof(Stock),
            typeof(int),
            typeof(StockBadge),
            new PropertyMetadata(0, OnStockChanged));

    public int Stock
    {
        get => (int)GetValue(StockProperty);
        set => SetValue(StockProperty, value);
    }

    public static readonly DependencyProperty LowStockThresholdProperty =
        DependencyProperty.Register(
            nameof(LowStockThreshold),
            typeof(int),
            typeof(StockBadge),
            new PropertyMetadata(10, OnStockChanged));

    public int LowStockThreshold
    {
        get => (int)GetValue(LowStockThresholdProperty);
        set => SetValue(LowStockThresholdProperty, value);
    }

    private static void OnStockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StockBadge badge)
        {
            badge.UpdateStockDisplay();
        }
    }

    private void UpdateStockDisplay()
    {
        string text;
        string background;
        string foreground;
        string icon;

        if (Stock == 0)
        {
            text = "Out of Stock";
            background = "#FEE2E2"; // Red light
            foreground = "#B91C1C"; // Red dark
            icon = "\uE711"; // Cancel icon
        }
        else if (Stock <= LowStockThreshold)
        {
            text = $"Low Stock ({Stock})";
            background = "#FEF3C7"; // Yellow light
            foreground = "#B45309"; // Yellow dark
            icon = "\uE7BA"; // Warning icon
        }
        else
        {
            text = $"In Stock ({Stock})";
            background = "#DCFCE7"; // Green light
            foreground = "#15803D"; // Green dark
            icon = "\uE73E"; // Checkmark icon
        }

        StockText.Text = text;
        StockContainer.Background = new SolidColorBrush(ColorHelper(background));
        StockText.Foreground = new SolidColorBrush(ColorHelper(foreground));
        StockIcon.Foreground = new SolidColorBrush(ColorHelper(foreground));
        StockIcon.Glyph = icon;
    }

    private Windows.UI.Color ColorHelper(string hex)
    {
        hex = hex.Replace("#", "");
        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        return Windows.UI.Color.FromArgb(255, r, g, b);
    }
}
