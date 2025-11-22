using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Repositories;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Admin;

public partial class AdminUsersViewModel : ObservableObject
{
    private readonly IUserRepository _userRepository;
    private List<UserViewModel> _allUsers = new();

    [ObservableProperty]
    private ObservableCollection<UserViewModel> _users;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedRole = "All Roles";

    [ObservableProperty]
    private string _selectedStatus = "All Status";

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalUsers = 0;

    public AdminUsersViewModel(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        Users = new ObservableCollection<UserViewModel>();
    }

    public async Task InitializeAsync()
    {
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            
            _allUsers = users.Select(u => {
                var roleString = u.GetPrimaryRole().ToString();
                var isActive = u.IsEmailVerified;
                
                return new UserViewModel
                {
                    Id = u.Id,
                    Name = u.FullName ?? u.Username,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.PhoneNumber ?? "N/A",
                    Role = roleString,
                    RoleColor = GetRoleColor(roleString),
                    RoleBgColor = GetRoleBgColor(roleString),
                    Status = isActive ? "Active" : "Inactive",
                    StatusColor = isActive ? "#10B981" : "#6B7280",
                    StatusBgColor = isActive ? "#D1FAE5" : "#F3F4F6",
                    IsActive = isActive,
                    FullName = u.FullName ?? u.Username
                };
            }).ToList();

            Users = new ObservableCollection<UserViewModel>(_allUsers);
            TotalUsers = _allUsers.Count;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Error loading users: {ex.Message}");
            _allUsers = new List<UserViewModel>();
            Users = new ObservableCollection<UserViewModel>();
        }
    }

    private string GetRoleColor(string role) => role switch
    {
        "Admin" => "#DC2626",
        "Salesman" => "#2563EB",
        "Customer" => "#10B981",
        _ => "#6B7280"
    };

    private string GetRoleBgColor(string role) => role switch
    {
        "Admin" => "#FEE2E2",
        "Salesman" => "#DBEAFE",
        "Customer" => "#D1FAE5",
        _ => "#F3F4F6"
    };

    [RelayCommand]
    private void Search(string query)
    {
        SearchQuery = query;
        ApplyFilters();
    }

    [RelayCommand]
    private void FilterByRole(string role)
    {
        SelectedRole = role;
        ApplyFilters();
    }

    [RelayCommand]
    private void FilterByStatus(string status)
    {
        SelectedStatus = status;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allUsers.AsEnumerable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = filtered.Where(u => 
                u.FullName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                u.Username.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
        }

        // Role filter
        if (!string.IsNullOrEmpty(SelectedRole) && SelectedRole != "All Roles")
        {
            filtered = filtered.Where(u => u.Role == SelectedRole);
        }

        // Status filter
        if (!string.IsNullOrEmpty(SelectedStatus) && SelectedStatus != "All Status")
        {
            filtered = filtered.Where(u => u.Status == SelectedStatus);
        }

        Users.Clear();
        foreach (var user in filtered)
        {
            Users.Add(user);
        }

        System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Filters applied - Found {Users.Count} users");
    }

    [RelayCommand]
    private async Task AddNewUserAsync()
    {
        try
        {
            var dialog = new Views.Dialogs.AddUserDialog
            {
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };
            
            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                try
                {
                    // Note: User creation handled by backend registration endpoint
                    // Just reload the list to show new user
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] New user added: {dialog.ViewModel.FullName}");
                    await LoadUsersAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Error saving user: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Error adding user: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ChangeRoleAsync(UserViewModel user)
    {
        // TODO: Implement ChangeRoleDialog when needed
        System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Change role for: {user.Name}");
    }

    [RelayCommand]
    private async Task ResetPasswordAsync(UserViewModel user)
    {
        // TODO: Implement ResetPasswordDialog when needed
        System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Reset password for: {user.Name}");
    }

    [RelayCommand]
    private async Task EditTaxRateAsync(UserViewModel user)
    {
        // TODO: Implement EditTaxRateDialog when needed (for sales agents)
        System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Edit tax rate for: {user.Name}");
    }

    [RelayCommand]
    private void ToggleUserStatus(UserViewModel user)
    {
        user.IsActive = !user.IsActive;
        user.Status = user.IsActive ? "Active" : "Inactive";
        user.StatusColor = user.IsActive ? "#10B981" : "#6B7280";
        user.StatusBgColor = user.IsActive ? "#D1FAE5" : "#F3F4F6";
    }

    [RelayCommand]
    private void GoToPage(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            _ = LoadUsersAsync();
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            _ = LoadUsersAsync();
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            _ = LoadUsersAsync();
        }
    }
}

public partial class UserViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private string _roleColor = string.Empty;

    [ObservableProperty]
    private string _roleBgColor = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _statusColor = string.Empty;

    [ObservableProperty]
    private string _statusBgColor = string.Empty;

    [ObservableProperty]
    private bool _isActive = true;
}
