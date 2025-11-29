using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MyShop.Client.ViewModels.Dialogs;

public partial class AddUserDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _selectedRole = "Customer";

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    public ObservableCollection<string> AvailableRoles { get; } = new()
    {
        "Customer",
        "Salesman",
        "Admin"
    };

    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(FullName) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Phone) ||
            string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            return false;
        }

        if (Password != ConfirmPassword)
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
        
        if (string.IsNullOrWhiteSpace(Password))
            return "Password is required";
        
        if (Password.Length < 6)
            return "Password must be at least 6 characters";
        
        if (Password != ConfirmPassword)
            return "Passwords do not match";

        return string.Empty;
    }
}
