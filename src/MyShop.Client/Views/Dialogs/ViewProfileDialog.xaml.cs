using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Dialogs;

/// <summary>
/// Dialog to view agent request profile details
/// </summary>
public sealed partial class ViewProfileDialog : ContentDialog
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string SubmittedDate { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string Experience { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    public ViewProfileDialog()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Create a dialog from an AgentRequestItem
    /// </summary>
    public static ViewProfileDialog FromAgentRequest(ViewModels.Admin.AgentRequestItem request)
    {
        return new ViewProfileDialog
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone ?? "N/A",
            AvatarUrl = request.Avatar,
            SubmittedDate = request.SubmittedDate,
            Status = request.Status,
            Experience = request.Experience,
            Reason = request.Reason
        };
    }
}
