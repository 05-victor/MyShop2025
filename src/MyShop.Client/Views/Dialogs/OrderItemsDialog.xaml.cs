using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Text;
using System.Collections.Generic;
using System.Linq;
using MyShop.Shared.Models;

namespace MyShop.Client.Views.Dialogs;

public sealed partial class OrderItemsDialog : ContentDialog
{
    public List<OrderItemRow> Items { get; set; }

    // Properties for order status and related callbacks
    public string? OrderStatus { get; set; }
    public string? OrderId { get; set; }
    public System.Func<System.Threading.Tasks.Task>? OnMarkAsProcessingAsync { get; set; }
    public System.Func<System.Threading.Tasks.Task>? OnMarkAsShippedAsync { get; set; }
    public System.Func<System.Threading.Tasks.Task>? OnMarkAsDeliveredAsync { get; set; }

    public OrderItemsDialog()
    {
        this.InitializeComponent();
        Items = new List<OrderItemRow>();
    }

    /// <summary>
    /// Initialize dialog with order items and status
    /// </summary>
    public void Initialize(List<OrderItem> orderItems, string? status = null, string? orderId = null)
    {
        Items = orderItems?
            .Select(item => new OrderItemRow
            {
                Name = item.ProductName ?? "Unknown",
                SKU = item.ProductSKU ?? "N/A",
                Quantity = item.Quantity,
                Price = item.UnitPrice,
                TotalPrice = item.TotalPrice
            })
            .ToList() ?? new List<OrderItemRow>();

        OrderStatus = status;
        OrderId = orderId;

        // Populate the UI with items
        PopulateItemsUI();
        UpdateActionButtons();
    }

    private void UpdateActionButtons()
    {
        // Clear any existing secondary/tertiary buttons
        SecondaryButtonText = null;

        if (OrderStatus?.Equals("Confirmed", System.StringComparison.OrdinalIgnoreCase) == true)
        {
            SecondaryButtonText = "Mark as Processing";
        }
        else if (OrderStatus?.Equals("Processing", System.StringComparison.OrdinalIgnoreCase) == true)
        {
            SecondaryButtonText = "Mark as shipped";
        }
        else if (OrderStatus?.Equals("Shipped", System.StringComparison.OrdinalIgnoreCase) == true)
        {
            SecondaryButtonText = "Mark as Delivered";
        }
    }

    private void PopulateItemsUI()
    {
        ItemsStackPanel.Children.Clear();

        // Add header row
        var headerGrid = CreateHeaderRow();
        ItemsStackPanel.Children.Add(headerGrid);

        // Add item rows
        foreach (var item in Items)
        {
            var itemBorder = CreateItemRow(item);
            ItemsStackPanel.Children.Add(itemBorder);
        }
    }

    private Grid CreateHeaderRow()
    {
        var grid = new Grid
        {
            Padding = new Thickness(12, 12, 12, 12),
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
        };

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(52) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(88) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(88) });

        var headers = new[] { "Name", "SKU", "Quantity", "Price", "Total" };
        var columns = new[] { 0, 1, 2, 3, 4 };

        for (int i = 0; i < headers.Length; i++)
        {
            var header = new TextBlock
            {
                Text = headers[i],
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 150, 150, 150)),
                TextAlignment = i >= 2 ? TextAlignment.Right : TextAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(header, columns[i]);
            grid.Children.Add(header);
        }

        return grid;
    }

    private Border CreateItemRow(OrderItemRow item)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 40, 40, 40)),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 60, 60, 60)),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(12, 12, 12, 12),
            CornerRadius = new CornerRadius(4)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(52) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(88) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(88) });

        // Name
        var nameBlock = new TextBlock
        {
            Text = item.Name,
            FontSize = 13,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Padding = new Thickness(0, 0, 16, 0)
        };
        Grid.SetColumn(nameBlock, 0);
        grid.Children.Add(nameBlock);

        // SKU
        var skuBlock = new TextBlock
        {
            Text = item.SKU,
            FontSize = 13,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 150, 150, 150)),
            FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };
        Grid.SetColumn(skuBlock, 1);
        grid.Children.Add(skuBlock);

        // Quantity
        var qtyBlock = new TextBlock
        {
            Text = item.Quantity.ToString(),
            FontSize = 13,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(qtyBlock, 2);
        grid.Children.Add(qtyBlock);

        // Price
        var priceBlock = new TextBlock
        {
            Text = FormatCurrency(item.Price),
            FontSize = 13,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(priceBlock, 3);
        grid.Children.Add(priceBlock);

        // Total Price
        var totalBlock = new TextBlock
        {
            Text = FormatCurrency(item.TotalPrice),
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(totalBlock, 4);
        grid.Children.Add(totalBlock);

        border.Child = grid;
        return border;
    }

    /// <summary>
    /// Format decimal value as currency with VND symbol
    /// Format: 1,234,567₫ with dot as thousand separator
    /// </summary>
    private string FormatCurrency(decimal amount)
    {
        try
        {
            amount = System.Math.Round(amount, 0, System.MidpointRounding.AwayFromZero);
            var nfi = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ".";
            nfi.NumberDecimalSeparator = ",";
            return amount.ToString("#,##0", nfi) + "₫";
        }
        catch
        {
            return "0₫";
        }
    }

    private void OnCloseClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Just close the dialog
    }

    private async void OnActionButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Close dialog FIRST before showing confirmation dialog
        // This prevents "Only a single ContentDialog can be open at any time" error
        this.Hide();

        // Wait a moment for the dialog to close
        await Task.Delay(100);

        // Handle action button click based on current status
        if (OrderStatus?.Equals("Confirmed", System.StringComparison.OrdinalIgnoreCase) == true)
        {
            if (OnMarkAsProcessingAsync != null)
            {
                await OnMarkAsProcessingAsync();
            }
        }
        else if (OrderStatus?.Equals("Processing", System.StringComparison.OrdinalIgnoreCase) == true)
        {
            if (OnMarkAsShippedAsync != null)
            {
                await OnMarkAsShippedAsync();
            }
        }
        else if (OrderStatus?.Equals("Shipped", System.StringComparison.OrdinalIgnoreCase) == true)
        {
            if (OnMarkAsDeliveredAsync != null)
            {
                await OnMarkAsDeliveredAsync();
            }
        }
    }
}

/// <summary>
/// Data model for displaying order items in dialog
/// </summary>
public class OrderItemRow
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
}

