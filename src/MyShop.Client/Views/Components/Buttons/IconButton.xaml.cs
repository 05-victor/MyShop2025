using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Windows.Input;
using Windows.UI;

namespace MyShop.Client.Views.Components.Buttons;

/// <summary>
/// A reusable button with icon and optional text.
/// Supports variants: Ghost, Outline, Filled.
/// </summary>
public sealed partial class IconButton : UserControl
{
    // Template children - accessed after template is applied
    private FontIcon? _leftIcon;
    private FontIcon? _rightIcon;
    private TextBlock? _buttonText;
    private StackPanel? _contentPanel;
    private bool _templateApplied = false;
    
    public IconButton()
    {
        this.InitializeComponent();
        ActionButton.Loaded += ActionButton_Loaded;
        ActionButton.LayoutUpdated += ActionButton_LayoutUpdated;
        // ✅ NO ThemeChanged subscription - styles handle theme automatically
    }

    private void ActionButton_Loaded(object sender, RoutedEventArgs e)
    {
        FindTemplateChildren();
        ApplyVariantStyle();
        ApplyLayout();
        _templateApplied = true;
    }

    private void ActionButton_LayoutUpdated(object? sender, object e)
    {
        // When tab switching causes Visibility changes, buttons re-render
        // but Loaded doesn't fire again. LayoutUpdated catches these cases.
        if (!_templateApplied || _leftIcon == null)
        {
            FindTemplateChildren();
            if (_leftIcon != null)
            {
                ApplyVariantStyle();
                ApplyLayout();
                _templateApplied = true;
            }
        }
    }

    private void FindTemplateChildren()
    {
        // Find template children using VisualTreeHelper
        _contentPanel = FindChildByName<StackPanel>(ActionButton, "ContentPanel");
        if (_contentPanel != null)
        {
            _leftIcon = FindChildByName<FontIcon>(_contentPanel, "LeftIcon");
            _rightIcon = FindChildByName<FontIcon>(_contentPanel, "RightIcon");
            _buttonText = FindChildByName<TextBlock>(_contentPanel, "ButtonText");
        }
    }

    // Helper method to find child element by name in visual tree
    private static T? FindChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        if (parent == null) return null;

        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is T typedChild && typedChild.Name == name)
            {
                return typedChild;
            }

            var result = FindChildByName<T>(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    #region IconGlyph Property

    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(
            nameof(IconGlyph),
            typeof(string),
            typeof(IconButton),
            new PropertyMetadata("\uE712", OnIconGlyphChanged)); // Default: More icon

    /// <summary>
    /// Gets or sets the icon glyph (Segoe Fluent Icons).
    /// </summary>
    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    private static void OnIconGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IconButton button)
        {
            button.ApplyLayout();
        }
    }

    #endregion

    #region Glyph Property (Backward Compatibility)

    public static readonly DependencyProperty GlyphProperty =
        DependencyProperty.Register(
            nameof(Glyph),
            typeof(string),
            typeof(IconButton),
            new PropertyMetadata(null, OnGlyphChanged));

    /// <summary>
    /// Gets or sets the icon glyph (legacy, use IconGlyph instead).
    /// </summary>
    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    private static void OnGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IconButton button && e.NewValue is string glyph && !string.IsNullOrEmpty(glyph))
        {
            button.IconGlyph = glyph;
        }
    }

    #endregion

    #region Text Property

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(IconButton),
            new PropertyMetadata(string.Empty, OnTextChanged));

    /// <summary>
    /// Gets or sets the button text. If empty, only icon is shown.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IconButton button)
        {
            button.ApplyLayout();
        }
    }

    #endregion

    #region TextColor Property

    public static readonly DependencyProperty TextColorProperty =
        DependencyProperty.Register(
            nameof(TextColor),
            typeof(Brush),
            typeof(IconButton),
            new PropertyMetadata(null, OnTextColorChanged));

    /// <summary>
    /// Gets or sets the text foreground color. If null, uses default.
    /// </summary>
    public Brush TextColor
    {
        get => (Brush)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    private static void OnTextColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IconButton button && e.NewValue is Brush brush && button._buttonText != null)
        {
            button._buttonText.Foreground = brush;
        }
    }

    #endregion

    #region IconPosition Property

    public static readonly DependencyProperty IconPositionProperty =
        DependencyProperty.Register(
            nameof(IconPosition),
            typeof(IconPosition),
            typeof(IconButton),
            new PropertyMetadata(IconPosition.Left, OnIconPositionChanged));

    /// <summary>
    /// Gets or sets the icon position (Left or Right).
    /// </summary>
    public IconPosition IconPosition
    {
        get => (IconPosition)GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
    }

    private static void OnIconPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IconButton button)
        {
            button.ApplyLayout();
        }
    }

    #endregion

    #region Tooltip Property

    public static readonly DependencyProperty TooltipProperty =
        DependencyProperty.Register(
            nameof(Tooltip),
            typeof(string),
            typeof(IconButton),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the tooltip text.
    /// </summary>
    public string Tooltip
    {
        get => (string)GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }

    #endregion

    #region Command Property

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(IconButton),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the command to execute when clicked.
    /// </summary>
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    #endregion

    #region CommandParameter Property

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(IconButton),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the command parameter.
    /// </summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    #endregion

    #region Variant Property

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(
            nameof(Variant),
            typeof(IconButtonVariant),
            typeof(IconButton),
            new PropertyMetadata(IconButtonVariant.Ghost, OnVariantChanged));

    /// <summary>
    /// Gets or sets the button variant (Ghost, Outline, Filled).
    /// </summary>
    public IconButtonVariant Variant
    {
        get => (IconButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IconButton button)
        {
            button.ApplyVariantStyle();
        }
    }

    #endregion

    #region IconColor Property

    public static readonly DependencyProperty IconColorProperty =
        DependencyProperty.Register(
            nameof(IconColor),
            typeof(Brush),
            typeof(IconButton),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)))); // #6B7280

    /// <summary>
    /// Gets or sets the icon foreground color.
    /// </summary>
    public Brush IconColor
    {
        get => (Brush)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    #endregion

    #region Size Property

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(
            nameof(Size),
            typeof(double),
            typeof(IconButton),
            new PropertyMetadata(double.NaN));

    /// <summary>
    /// Gets or sets the button size (width and height). Use NaN for auto-sizing.
    /// </summary>
    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    #endregion

    #region IconSize Property

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(
            nameof(IconSize),
            typeof(double),
            typeof(IconButton),
            new PropertyMetadata(14.0));

    /// <summary>
    /// Gets or sets the icon font size.
    /// </summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    #endregion

    #region Click Event

    public event RoutedEventHandler Click;

    private void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        Click?.Invoke(this, e);
    }

    #endregion

    private void ApplyLayout()
    {
        if (ActionButton == null || _leftIcon == null || _rightIcon == null || _buttonText == null) return;

        bool hasText = !string.IsNullOrEmpty(Text);
        bool hasIcon = !string.IsNullOrEmpty(IconGlyph);

        // Show/hide text
        _buttonText.Visibility = hasText ? Visibility.Visible : Visibility.Collapsed;

        // Apply icon position
        if (hasIcon && hasText)
        {
            if (IconPosition == IconPosition.Right)
            {
                _leftIcon.Visibility = Visibility.Collapsed;
                _rightIcon.Visibility = Visibility.Visible;
            }
            else
            {
                _leftIcon.Visibility = Visibility.Visible;
                _rightIcon.Visibility = Visibility.Collapsed;
            }
            // Text + Icon: use standard padding
            ActionButton.Padding = new Thickness(16, 10, 16, 10);
            ActionButton.Width = double.NaN;
            ActionButton.Height = double.NaN;
        }
        else if (hasIcon && !hasText)
        {
            // Icon only: square button
            _leftIcon.Visibility = Visibility.Visible;
            _rightIcon.Visibility = Visibility.Collapsed;
            ActionButton.Padding = new Thickness(8);
            if (!double.IsNaN(Size))
            {
                ActionButton.Width = Size;
                ActionButton.Height = Size;
            }
            else
            {
                ActionButton.Width = 36;
                ActionButton.Height = 36;
            }
        }
        else
        {
            // Text only
            _leftIcon.Visibility = Visibility.Collapsed;
            _rightIcon.Visibility = Visibility.Collapsed;
            ActionButton.Padding = new Thickness(16, 10, 16, 10);
            ActionButton.Width = double.NaN;
            ActionButton.Height = double.NaN;
        }
    }

    private void ApplyVariantStyle()
    {
        if (ActionButton == null) return;

        // ✅ Microsoft pattern: Apply style based on variant
        // Styles are defined in ButtonStyles.xaml with ThemeResource
        var styleKey = Variant switch
        {
            IconButtonVariant.Ghost => "IconButtonGhostStyle",
            IconButtonVariant.Outline => "IconButtonOutlineStyle",
            IconButtonVariant.Filled => "IconButtonFilledStyle",
            _ => "IconButtonGhostStyle"
        };

        if (Application.Current.Resources.TryGetValue(styleKey, out var style) 
            && style is Style buttonStyle)
        {
            ActionButton.Style = buttonStyle;
        }
        
        // For Filled variant, ensure white icons if not explicitly set
        if (Variant == IconButtonVariant.Filled && 
            IconColor is SolidColorBrush brush && 
            brush.Color == Color.FromArgb(255, 107, 114, 128))
        {
            if (_leftIcon != null) _leftIcon.Foreground = new SolidColorBrush(Colors.White);
            if (_rightIcon != null) _rightIcon.Foreground = new SolidColorBrush(Colors.White);
        }
    }
}

/// <summary>
/// Defines the visual variants for IconButton.
/// </summary>
public enum IconButtonVariant
{
    /// <summary>
    /// Transparent background, no border.
    /// </summary>
    Ghost,

    /// <summary>
    /// White background with border.
    /// </summary>
    Outline,

    /// <summary>
    /// Filled primary color background.
    /// </summary>
    Filled
}

/// <summary>
/// Defines the icon position for IconButton.
/// </summary>
public enum IconPosition
{
    /// <summary>
    /// Icon on the left side of text.
    /// </summary>
    Left,

    /// <summary>
    /// Icon on the right side of text.
    /// </summary>
    Right
}
