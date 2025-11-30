using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

namespace MyShop.Client.Views.Components.Layout;

/// <summary>
/// A reusable icon + label + value row for detail views.
/// </summary>
public sealed partial class DetailRow : UserControl
{
    public DetailRow()
    {
        this.InitializeComponent();
        this.PointerPressed += OnPointerPressed;
    }

    #region IconGlyph Property

    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(
            nameof(IconGlyph),
            typeof(string),
            typeof(DetailRow),
            new PropertyMetadata("\uE946")); // Default: Info icon

    /// <summary>
    /// Gets or sets the icon glyph.
    /// </summary>
    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    #endregion

    #region Label Property

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(DetailRow),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the label text.
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    #endregion

    #region Value Property

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(DetailRow),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the value text.
    /// </summary>
    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    #endregion

    #region IsClickable Property

    public static readonly DependencyProperty IsClickableProperty =
        DependencyProperty.Register(
            nameof(IsClickable),
            typeof(bool),
            typeof(DetailRow),
            new PropertyMetadata(false, OnIsClickableChanged));

    /// <summary>
    /// Gets or sets whether the row is clickable (shows as link).
    /// </summary>
    public bool IsClickable
    {
        get => (bool)GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }

    private static void OnIsClickableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DetailRow row && (bool)e.NewValue)
        {
            row.ValueText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 37, 99, 235)); // #2563EB
            row.ValueText.TextDecorations = Windows.UI.Text.TextDecorations.Underline;
        }
    }

    #endregion

    #region Click Event

    public event EventHandler<string> Click;

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (IsClickable)
        {
            Click?.Invoke(this, Value);
        }
    }

    #endregion
}
