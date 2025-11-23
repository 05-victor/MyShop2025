using CommunityToolkit.Mvvm.ComponentModel;
using MyShop.Shared.Models;
using System.Collections.ObjectModel;

namespace MyShop.Client.ViewModels.Dialogs;

public partial class EditUserDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _userId = string.Empty;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _selectedRole = "Customer";

    [ObservableProperty]
    private string _selectedStatus = "Active";

    public ObservableCollection<string> AvailableRoles { get; } = new()
    {
        "Customer",
        "Sales Agent",
        "Admin"
    };

    public ObservableCollection<string> AvailableStatuses { get; } = new()
    {
        "Active",
        "Inactive",
        "Suspended"
    };

    public void LoadUser(User? user)
    {
        if (user == null) return;
        
        UserId = user.Id.ToString();
        FullName = user.FullName ?? string.Empty;
        Email = user.Email;
        Phone = user.PhoneNumber ?? string.Empty;
        SelectedRole = user.GetPrimaryRole().ToString();
        SelectedStatus = "Active"; // Default, can be extended from User model
    }

    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(FullName) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Phone))
        {
            return false;
        }

        // Basic email validation
        if (!Email.Contains("@") || !Email.Contains("."))
        {
            return false;
        }

        return true;
    }

    public string GetValidationError()
    {
        if (string.IsNullOrWhiteSpace(FullName))
            return "Full name is required";
        
        if (string.IsNullOrWhiteSpace(Email))
            return "Email is required";
        
        if (!Email.Contains("@") || !Email.Contains("."))
            return "Invalid email format";
        
        if (string.IsNullOrWhiteSpace(Phone))
            return "Phone is required";

        return string.Empty;
    }
}
