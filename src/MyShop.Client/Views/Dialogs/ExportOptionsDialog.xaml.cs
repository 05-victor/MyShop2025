using Microsoft.UI.Xaml.Controls;
using System;

namespace MyShop.Client.Views.Dialogs;

/// <summary>
/// Dialog for selecting export format and options.
/// </summary>
public sealed partial class ExportOptionsDialog : ContentDialog
{
    public ExportOptionsDialog()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets the selected export format.
    /// </summary>
    public string SelectedFormat
    {
        get
        {
            var selected = FormatRadioButtons.SelectedItem as RadioButton;
            return selected?.Tag?.ToString() ?? "csv";
        }
    }

    /// <summary>
    /// Gets the start date filter (if any).
    /// </summary>
    public DateTimeOffset? StartDate => StartDatePicker.Date;

    /// <summary>
    /// Gets the end date filter (if any).
    /// </summary>
    public DateTimeOffset? EndDate => EndDatePicker.Date;

    /// <summary>
    /// Whether to include column headers.
    /// </summary>
    public bool IncludeHeaders => IncludeHeadersCheckBox.IsChecked ?? true;

    /// <summary>
    /// Whether to include metadata in export.
    /// </summary>
    public bool IncludeMetadata => IncludeMetadataCheckBox.IsChecked ?? false;

    /// <summary>
    /// Shows the date range section.
    /// </summary>
    public void ShowDateRange()
    {
        DateRangeSection.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
    }

    /// <summary>
    /// Updates the preview text.
    /// </summary>
    public void SetPreviewText(string text)
    {
        PreviewText.Text = text;
    }
}
