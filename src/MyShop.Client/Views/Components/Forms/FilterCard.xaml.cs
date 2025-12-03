using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

namespace MyShop.Client.Views.Components.Forms;

/// <summary>
/// A reusable filter section card with title, filter controls slot, and action buttons.
/// </summary>
public sealed partial class FilterCard : UserControl
{
    public FilterCard()
    {
        this.InitializeComponent();
    }

    #region Title Property

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(FilterCard),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the filter card title.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    #endregion

    #region FilterContent Property

    public static readonly DependencyProperty FilterContentProperty =
        DependencyProperty.Register(
            nameof(FilterContent),
            typeof(object),
            typeof(FilterCard),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the filter controls content (Grid with ComboBoxes, TextBoxes, etc.).
    /// </summary>
    public object FilterContent
    {
        get => GetValue(FilterContentProperty);
        set => SetValue(FilterContentProperty, value);
    }

    #endregion

    #region ShowActions Property

    public static readonly DependencyProperty ShowActionsProperty =
        DependencyProperty.Register(
            nameof(ShowActions),
            typeof(bool),
            typeof(FilterCard),
            new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets whether to show the action buttons (Apply, Reset).
    /// </summary>
    public bool ShowActions
    {
        get => (bool)GetValue(ShowActionsProperty);
        set => SetValue(ShowActionsProperty, value);
    }

    #endregion

    #region ShowResetButton Property

    public static readonly DependencyProperty ShowResetButtonProperty =
        DependencyProperty.Register(
            nameof(ShowResetButton),
            typeof(bool),
            typeof(FilterCard),
            new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets whether to show the Reset button.
    /// </summary>
    public bool ShowResetButton
    {
        get => (bool)GetValue(ShowResetButtonProperty);
        set => SetValue(ShowResetButtonProperty, value);
    }

    #endregion

    #region ApplyCommand Property

    public static readonly DependencyProperty ApplyCommandProperty =
        DependencyProperty.Register(
            nameof(ApplyCommand),
            typeof(ICommand),
            typeof(FilterCard),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the command to execute when Apply is clicked.
    /// </summary>
    public ICommand ApplyCommand
    {
        get => (ICommand)GetValue(ApplyCommandProperty);
        set => SetValue(ApplyCommandProperty, value);
    }

    #endregion

    #region ResetCommand Property

    public static readonly DependencyProperty ResetCommandProperty =
        DependencyProperty.Register(
            nameof(ResetCommand),
            typeof(ICommand),
            typeof(FilterCard),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the command to execute when Reset is clicked.
    /// </summary>
    public ICommand ResetCommand
    {
        get => (ICommand)GetValue(ResetCommandProperty);
        set => SetValue(ResetCommandProperty, value);
    }

    #endregion

    #region ApplyButtonText Property

    public static readonly DependencyProperty ApplyButtonTextProperty =
        DependencyProperty.Register(
            nameof(ApplyButtonText),
            typeof(string),
            typeof(FilterCard),
            new PropertyMetadata("Apply Filters"));

    /// <summary>
    /// Gets or sets the Apply button text.
    /// </summary>
    public string ApplyButtonText
    {
        get => (string)GetValue(ApplyButtonTextProperty);
        set => SetValue(ApplyButtonTextProperty, value);
    }

    #endregion
}
