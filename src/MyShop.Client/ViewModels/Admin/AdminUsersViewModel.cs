using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Admin;

/// <summary>
/// ViewModel for Admin Users management with server-side paging
/// Extends PagedViewModelBase to inherit paging logic
/// </summary>
public partial class AdminUsersViewModel : PagedViewModelBase<UserViewModel>
{
    private readonly IUserFacade _userFacade;

    [ObservableProperty]
    private string _selectedRole = "All Roles";

    [ObservableProperty]
    private string _selectedStatus = "All Status";

    public AdminUsersViewModel(
        IUserFacade userFacade,
        IToastService toastService,
        INavigationService navigationService)
        : base(toastService, navigationService)
    {
        _userFacade = userFacade;
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    /// <summary>
    /// Reload data when filters change
    /// </summary>
    partial void OnSelectedRoleChanged(string value)
    {
        CurrentPage = 1;
        _ = LoadPageAsync();
    }

    partial void OnSelectedStatusChanged(string value)
    {
        CurrentPage = 1;
        _ = LoadPageAsync();
    }

    /// <summary>
    /// Override LoadPageAsync to fetch users with server-side paging
    /// </summary>
    protected override async Task LoadPageAsync()
    {
        try
        {
            SetLoadingState(true);

            // Get filter parameters
            var roleFilter = SelectedRole == "All Roles" ? null : SelectedRole;
            var isActive = SelectedStatus == "All Status" ? (bool?)null : SelectedStatus == "Active";

            // Call facade with paging parameters
            var result = await _userFacade.LoadUsersAsync(
                page: CurrentPage,
                pageSize: PageSize,
                searchQuery: SearchQuery,
                role: roleFilter,
                isActive: isActive);

            if (!result.IsSuccess || result.Data == null)
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to load users");
                Items.Clear();
                UpdatePagingInfo(0);
                return;
            }

            // Map DTOs to ViewModels
            var users = result.Data.Items.Select(u =>
            {
                var roleString = u.GetPrimaryRole().ToString();
                var isActiveUser = u.IsEmailVerified;

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
                    Status = isActiveUser ? "Active" : "Inactive",
                    StatusColor = isActiveUser ? "#10B981" : "#6B7280",
                    StatusBgColor = isActiveUser ? "#D1FAE5" : "#F3F4F6",
                    IsActive = isActiveUser,
                    FullName = u.FullName ?? u.Username,
                    Avatar = u.Avatar ?? string.Empty
                };
            }).ToList();

            Items.Clear();
            foreach (var user in users)
            {
                Items.Add(user);
            }

            UpdatePagingInfo(result.Data.TotalCount);

            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Loaded page {CurrentPage}/{TotalPages} ({Items.Count} items, {TotalItems} total)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Error loading users: {ex.Message}");
            await _toastHelper?.ShowError($"Error loading users: {ex.Message}");
            Items.Clear();
            UpdatePagingInfo(0);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private static string GetRoleColor(string role) => role switch
    {
        "Admin" => "#DC2626",
        "Salesman" => "#2563EB",
        "Customer" => "#10B981",
        _ => "#6B7280"
    };

    private static string GetRoleBgColor(string role) => role switch
    {
        "Admin" => "#FEE2E2",
        "Salesman" => "#DBEAFE",
        "Customer" => "#D1FAE5",
        _ => "#F3F4F6"
    };

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
                    await RefreshAsync();
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
    private async Task ToggleUserStatusAsync(UserViewModel user)
    {
        try
        {
            var result = await _userFacade.ToggleUserStatusAsync(user.Id);
            if (result.IsSuccess)
            {
                user.IsActive = !user.IsActive;
                user.Status = user.IsActive ? "Active" : "Inactive";
                user.StatusColor = user.IsActive ? "#10B981" : "#6B7280";
                user.StatusBgColor = user.IsActive ? "#D1FAE5" : "#F3F4F6";
                await _toastHelper?.ShowSuccess($"User {user.Name} status updated");
            }
            else
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to update user status");
            }
        }
        catch (Exception ex)
        {
            await _toastHelper?.ShowError($"Error updating user status: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ChangeRoleAsync(UserViewModel user)
    {
        // TODO: Implement ChangeRoleDialog when needed
        System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Change role for: {user.Name}");
        await _toastHelper?.ShowInfo("Change role feature coming soon");
    }

    [RelayCommand]
    private async Task ResetPasswordAsync(UserViewModel user)
    {
        // TODO: Implement ResetPasswordDialog when needed
        System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Reset password for: {user.Name}");
        await _toastHelper?.ShowInfo("Reset password feature coming soon");
    }

    [RelayCommand]
    private async Task EditTaxRateAsync(UserViewModel user)
    {
        // TODO: Implement EditTaxRateDialog when needed (for sales agents)
        System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Edit tax rate for: {user.Name}");
        await _toastHelper?.ShowInfo("Edit tax rate feature coming soon");
    }

    [RelayCommand]
    private async Task ExportUsersAsync()
    {
        SetLoadingState(true);
        try
        {
            var roleFilter = SelectedRole == "All Roles" ? null : SelectedRole;

            var result = await _userFacade.ExportUsersAsync(
                searchQuery: SearchQuery,
                roleFilter: roleFilter);

            if (result.IsSuccess)
            {
                await _toastHelper?.ShowSuccess($"Users exported to: {result.Data}");
            }
            else
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Export failed");
            }
        }
        catch (Exception ex)
        {
            await _toastHelper?.ShowError($"Export error: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
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

    [ObservableProperty]
    private string _avatar = string.Empty;
}
