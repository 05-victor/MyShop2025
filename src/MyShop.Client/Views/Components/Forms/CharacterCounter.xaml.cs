using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace MyShop.Client.Views.Components.Forms;

/// <summary>
/// Character counter component for text fields with optional max length.
/// </summary>
public sealed partial class CharacterCounter : UserControl
{
    private static readonly SolidColorBrush NormalBrush = new(Windows.UI.Color.FromArgb(255, 107, 114, 128));  // Gray
    private static readonly SolidColorBrush WarningBrush = new(Windows.UI.Color.FromArgb(255, 245, 158, 11)); // Amber
    private static readonly SolidColorBrush ErrorBrush = new(Windows.UI.Color.FromArgb(255, 220, 38, 38));    // Red

    public CharacterCounter()
    {
        this.InitializeComponent();
        UpdateCounter();
    }

    /// <summary>
    /// The text to count characters from.
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(CharacterCounter),
            new PropertyMetadata(string.Empty, OnTextChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Maximum allowed characters (0 = no limit).
    /// </summary>
    public static readonly DependencyProperty MaxLengthProperty =
        DependencyProperty.Register(
            nameof(MaxLength),
            typeof(int),
            typeof(CharacterCounter),
            new PropertyMetadata(0, OnMaxLengthChanged));

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    /// <summary>
    /// Warning threshold percentage (default 80%).
    /// </summary>
    public static readonly DependencyProperty WarningThresholdProperty =
        DependencyProperty.Register(
            nameof(WarningThreshold),
            typeof(double),
            typeof(CharacterCounter),
            new PropertyMetadata(0.8));

    public double WarningThreshold
    {
        get => (double)GetValue(WarningThresholdProperty);
        set => SetValue(WarningThresholdProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CharacterCounter counter)
            counter.UpdateCounter();
    }

    private static void OnMaxLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CharacterCounter counter)
            counter.UpdateCounter();
    }

    private void UpdateCounter()
    {
        var length = Text?.Length ?? 0;
        
        if (MaxLength > 0)
        {
            CounterText.Text = $"{length}/{MaxLength}";
            
            // Update color based on usage
            var percentage = (double)length / MaxLength;
            
            if (length > MaxLength)
            {
                CounterText.Foreground = ErrorBrush;
            }
            else if (percentage >= WarningThreshold)
            {
                CounterText.Foreground = WarningBrush;
            }
            else
            {
                CounterText.Foreground = NormalBrush;
            }
        }
        else
        {
            CounterText.Text = $"{length}";
            CounterText.Foreground = NormalBrush;
        }
    }
}
