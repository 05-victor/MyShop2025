using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace MyShop.Client.Views.Components.Tables;

/// <summary>
/// A reusable table header row with consistent styling.
/// </summary>
public sealed partial class TableHeader : UserControl
{
    public TableHeader()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        // Initialize Columns collection so XAML can add items to it
        Columns = new List<string>();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BuildHeaderFromColumns();
    }

    #region Columns Property

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(
            nameof(Columns),
            typeof(IList<string>),
            typeof(TableHeader),
            new PropertyMetadata(null, OnColumnsChanged));

    /// <summary>
    /// Gets or sets the column definitions as strings.
    /// Format: "Width|Header" or "Width|Header|Alignment"
    /// Examples: "3*|USER", "120|ORDER ID", "*|STATUS|Right"
    /// </summary>
    public IList<string> Columns
    {
        get => (IList<string>)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TableHeader header)
        {
            header.BuildHeaderFromColumns();
        }
    }

    #endregion

    #region HeaderContent Property

    public static readonly DependencyProperty HeaderContentProperty =
        DependencyProperty.Register(
            nameof(HeaderContent),
            typeof(object),
            typeof(TableHeader),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the header content (Grid with column definitions).
    /// </summary>
    public object HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    #endregion

    private void BuildHeaderFromColumns()
    {
        if (Columns == null || Columns.Count == 0 || HeaderGrid == null)
            return;

        HeaderGrid.ColumnDefinitions.Clear();
        HeaderGrid.Children.Clear();

        int colIndex = 0;
        foreach (var colDef in Columns)
        {
            // Parse format: "Width|Header" or "Width|Header|Alignment"
            var parts = colDef.Split('|');
            if (parts.Length < 2) continue;

            var widthStr = parts[0].Trim();
            var headerText = parts[1].Trim();
            var alignment = parts.Length > 2 ? parts[2].Trim() : "Left";

            // Create ColumnDefinition
            var columnDef = new ColumnDefinition();
            if (widthStr.EndsWith("*"))
            {
                if (widthStr == "*")
                {
                    columnDef.Width = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    var starValue = double.Parse(widthStr.TrimEnd('*'));
                    columnDef.Width = new GridLength(starValue, GridUnitType.Star);
                }
            }
            else if (double.TryParse(widthStr, out var pixelWidth))
            {
                columnDef.Width = new GridLength(pixelWidth);
            }
            else
            {
                columnDef.Width = new GridLength(1, GridUnitType.Star);
            }
            HeaderGrid.ColumnDefinitions.Add(columnDef);

            // Create TextBlock with theme-aware foreground
            var textBlock = new TextBlock
            {
                Text = headerText,
                FontSize = 11,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            // Use TextFillColorSecondaryBrush from theme resources for proper dark theme support
            if (Application.Current.Resources.TryGetValue("TextFillColorSecondaryBrush", out var brush) 
                && brush is Microsoft.UI.Xaml.Media.Brush themeBrush)
            {
                textBlock.Foreground = themeBrush;
            }
            else
            {
                // Fallback to default gray if theme resource not found
                textBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 107, 114, 128));
            }

            // Set alignment
            textBlock.HorizontalAlignment = alignment.ToLower() switch
            {
                "right" => HorizontalAlignment.Right,
                "center" => HorizontalAlignment.Center,
                _ => HorizontalAlignment.Left
            };

            Grid.SetColumn(textBlock, colIndex);
            HeaderGrid.Children.Add(textBlock);

            colIndex++;
        }
    }
}

/// <summary>
/// Represents a column definition for TableHeader.
/// </summary>
public class TableColumn
{
    /// <summary>
    /// Gets or sets the header text.
    /// </summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the column width (e.g., "3*", "*", "100").
    /// </summary>
    public string Width { get; set; } = "*";

    /// <summary>
    /// Gets or sets the horizontal alignment of the header text.
    /// </summary>
    public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Left;
}
