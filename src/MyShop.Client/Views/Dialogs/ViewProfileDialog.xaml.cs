using Microsoft.UI.Xaml.Controls;
using MyShop.Client.Views.Components.Badges;

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

    /// <summary>
    /// Gets the StatusBadge variant based on the current status
    /// </summary>
    public StatusBadgeVariant StatusVariant
    {
        get
        {
            if (string.IsNullOrEmpty(Status))
                return StatusBadgeVariant.Default;

            // Normalize status: trim whitespace and convert to uppercase for consistent matching
            var normalizedStatus = Status.Trim().ToUpperInvariant();

            return normalizedStatus switch
            {
                "PENDING" => StatusBadgeVariant.Pending,
                "APPROVED" => StatusBadgeVariant.Approved,
                "REJECTED" => StatusBadgeVariant.Rejected,
                _ => StatusBadgeVariant.Default
            };
        }
    }

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
