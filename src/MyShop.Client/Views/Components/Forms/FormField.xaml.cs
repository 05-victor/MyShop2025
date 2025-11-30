using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Components.Forms;

/// <summary>
/// A reusable form field component with label, input slot, help text, and error message.
/// </summary>
public sealed partial class FormField : UserControl
{
    public FormField()
    {
        this.InitializeComponent();
    }

    #region Label Property

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(FormField),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the field label text.
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    #endregion

    #region IsRequired Property

    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.Register(
            nameof(IsRequired),
            typeof(bool),
            typeof(FormField),
            new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets whether the field is required (shows * indicator).
    /// </summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    #endregion

    #region InputContent Property

    public static readonly DependencyProperty InputContentProperty =
        DependencyProperty.Register(
            nameof(InputContent),
            typeof(object),
            typeof(FormField),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the input control content (TextBox, ComboBox, PasswordBox, etc.).
    /// </summary>
    public object InputContent
    {
        get => GetValue(InputContentProperty);
        set => SetValue(InputContentProperty, value);
    }

    #endregion

    #region HelpText Property

    public static readonly DependencyProperty HelpTextProperty =
        DependencyProperty.Register(
            nameof(HelpText),
            typeof(string),
            typeof(FormField),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the help text shown below the input.
    /// </summary>
    public string HelpText
    {
        get => (string)GetValue(HelpTextProperty);
        set => SetValue(HelpTextProperty, value);
    }

    #endregion

    #region ErrorMessage Property

    public static readonly DependencyProperty ErrorMessageProperty =
        DependencyProperty.Register(
            nameof(ErrorMessage),
            typeof(string),
            typeof(FormField),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the error message shown when validation fails.
    /// </summary>
    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    #endregion
}
