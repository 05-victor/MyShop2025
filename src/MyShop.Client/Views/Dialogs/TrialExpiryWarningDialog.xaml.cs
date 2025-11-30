using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Dialogs;

/// <summary>
/// Dialog shown when admin's trial license is about to expire (≤1 day remaining).
/// Prompts user to extend trial or upgrade to permanent license.
/// </summary>
public sealed partial class TrialExpiryWarningDialog : ContentDialog
{
    /// <summary>
    /// Gets or sets the number of days remaining in the trial.
    /// </summary>
    public int DaysRemaining
    {
        get => int.TryParse(DaysRemainingRun.Text, out var days) ? days : 1;
        set => DaysRemainingRun.Text = value.ToString();
    }

    public TrialExpiryWarningDialog()
    {
        this.InitializeComponent();
    }

    public TrialExpiryWarningDialog(int daysRemaining) : this()
    {
        DaysRemaining = daysRemaining;
    }
}
