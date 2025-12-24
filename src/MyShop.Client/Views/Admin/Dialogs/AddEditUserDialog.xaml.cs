using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Admin;
using System;
using System.Text.RegularExpressions;

namespace MyShop.Client.Views.Admin.Dialogs;

/// <summary>
/// Add/Edit User Dialog with form validation
/// </summary>
public sealed partial class AddEditUserDialog : ContentDialog
{
    private bool _isEditMode = false;
    private UserViewModel? _existingUser = null;

    public UserViewModel? ResultUser { get; private set; }

    /// <summary>
    /// Constructor for Add mode
    /// </summary>
    public AddEditUserDialog()
    {
        this.InitializeComponent();
        _isEditMode = false;
        Title = "Add User";
        PrimaryButtonText = "Add User";
        
        // Password required in Add mode
        PasswordSection.Visibility = Visibility.Visible;
        ChangePasswordCheckBox.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Constructor for Edit mode
    /// </summary>
    public AddEditUserDialog(UserViewModel user)
    {
        this.InitializeComponent();
        _isEditMode = true;
        _existingUser = user;
        Title = "Edit User";
        PrimaryButtonText = "Save Changes";

        // Pre-populate fields
        FullNameTextBox.Text = user.Name ?? string.Empty;
        UsernameTextBox.Text = user.Username ?? string.Empty;
        EmailTextBox.Text = user.Email ?? string.Empty;
        PhoneTextBox.Text = user.Phone ?? string.Empty;
        AddressTextBox.Text = string.Empty; // Address not in UserViewModel

        // Set role
        var roleTag = user.Role switch
        {
            "Admin" => "Admin",
            "SalesAgent" => "SalesAgent",
            _ => "Customer"
        };
        foreach (ComboBoxItem item in RoleComboBox.Items)
        {
            if (item.Tag?.ToString() == roleTag)
            {
                RoleComboBox.SelectedItem = item;
                break;
            }
        }

        // Username not editable in Edit mode
        UsernameTextBox.IsReadOnly = true;
        UsernameTextBox.Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Microsoft.UI.Xaml.Media.Brush;

        // Password optional in Edit mode
        PasswordSection.Visibility = Visibility.Collapsed;
        ChangePasswordCheckBox.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Validate all fields on input change
    /// </summary>
    private void ValidationField_Changed(object sender, object e)
    {
        ValidateForm();
    }

    /// <summary>
    /// Toggle password section in Edit mode
    /// </summary>
    private void ChangePasswordCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        PasswordSection.Visibility = Visibility.Visible;
        ValidateForm();
    }

    private void ChangePasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        PasswordSection.Visibility = Visibility.Collapsed;
        PasswordBox.Password = string.Empty;
        PasswordError.Visibility = Visibility.Collapsed;
        ValidateForm();
    }

    /// <summary>
    /// Validate entire form and enable/disable Primary button
    /// </summary>
    private bool ValidateForm()
    {
        bool isValid = true;

        // Validate Full Name
        if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
        {
            FullNameError.Text = "Full name is required";
            FullNameError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            FullNameError.Visibility = Visibility.Collapsed;
        }

        // Validate Username (only in Add mode)
        if (!_isEditMode)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                UsernameError.Text = "Username is required";
                UsernameError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (UsernameTextBox.Text.Length < 3)
            {
                UsernameError.Text = "Username must be at least 3 characters";
                UsernameError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                UsernameError.Visibility = Visibility.Collapsed;
            }
        }

        // Validate Email
        if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
        {
            EmailError.Text = "Email is required";
            EmailError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else if (!IsValidEmail(EmailTextBox.Text))
        {
            EmailError.Text = "Invalid email format";
            EmailError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            EmailError.Visibility = Visibility.Collapsed;
        }

        // Validate Role
        if (RoleComboBox.SelectedItem == null)
        {
            RoleError.Text = "Role is required";
            RoleError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            RoleError.Visibility = Visibility.Collapsed;
        }

        // Validate Password (Add mode or Edit mode with ChangePassword checked)
        bool passwordRequired = !_isEditMode || (ChangePasswordCheckBox.IsChecked == true);
        if (passwordRequired)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordError.Text = "Password is required";
                PasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (PasswordBox.Password.Length < 6)
            {
                PasswordError.Text = "Password must be at least 6 characters";
                PasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                PasswordError.Visibility = Visibility.Collapsed;
            }
        }

        IsPrimaryButtonEnabled = isValid;
        return isValid;
    }

    /// <summary>
    /// Email validation using regex
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Handle Primary button click (Add/Save)
    /// </summary>
    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Validate one final time
        if (!ValidateForm())
        {
            args.Cancel = true;
            return;
        }

        // Create result user
        var selectedRole = (RoleComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Customer";

        if (_isEditMode && _existingUser != null)
        {
            // Update existing user
            ResultUser = _existingUser;
            ResultUser.Name = FullNameTextBox.Text.Trim();
            ResultUser.Email = EmailTextBox.Text.Trim();
            ResultUser.Phone = PhoneTextBox.Text.Trim();
            // Address not in UserViewModel - would need API call to update
            // Role update would need API call
            // Password update only if ChangePassword checked
        }
        else
        {
            // Create new user
            ResultUser = new UserViewModel
            {
                Name = FullNameTextBox.Text.Trim(),
                Username = UsernameTextBox.Text.Trim(),
                Email = EmailTextBox.Text.Trim(),
                Phone = PhoneTextBox.Text.Trim(),
                // Address not in UserViewModel - would be sent to API separately
                // Role and Password would be sent to API separately
            };
        }
    }
}
