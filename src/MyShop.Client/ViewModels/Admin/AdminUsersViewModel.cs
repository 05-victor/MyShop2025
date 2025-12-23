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

    // Applied filter values (used for actual API calls)
    [ObservableProperty]
    private string _selectedRole = "All Roles";

    [ObservableProperty]
    private string _selectedStatus = "All Status";

    // Pending filter values (user selections before Apply)
    [ObservableProperty]
    private string _pendingRole = "All Roles";

    [ObservableProperty]
    private string _pendingStatus = "All Status";

    [ObservableProperty]
    private string _pendingSearchQuery = string.Empty;

    // State for fetching user details (separate from list loading)
    [ObservableProperty]
    private bool _isDetailsLoading = false;

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
    /// Apply pending filters and reload data - reduces API calls
    /// </summary>
    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        // Transfer pending values to applied values
        SelectedRole = PendingRole;
        SelectedStatus = PendingStatus;
        SearchQuery = PendingSearchQuery;
        CurrentPage = 1;
        await LoadPageAsync();
    }

    /// <summary>
    /// Reset all filters to default values
    /// </summary>
    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        PendingRole = "All Roles";
        PendingStatus = "All Status";
        PendingSearchQuery = string.Empty;
        SelectedRole = "All Roles";
        SelectedStatus = "All Status";
        SearchQuery = string.Empty;
        CurrentPage = 1;
        await LoadPageAsync();
    }

    /// <summary>
    /// Override LoadPageAsync to fetch users with server-side paging
    /// </summary>
    protected override async Task LoadPageAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] LoadPageAsync START - CurrentPage: {CurrentPage}");
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
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] LoadPageAsync FAILED: {result.ErrorMessage}");
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to load users");
                Items.Clear();
                UpdatePagingInfo(0);
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] LoadPageAsync SUCCESS: {result.Data.Items.Count} items loaded");

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

            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] LoadPageAsync END - Loaded page {CurrentPage}/{TotalPages} ({Items.Count} items, {TotalItems} total)");
            SetLoadingState(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] LoadPageAsync EXCEPTION: {ex.Message}");
            await _toastHelper?.ShowError($"Error loading users: {ex.Message}");
            Items.Clear();
            UpdatePagingInfo(0);
            SetLoadingState(false);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private static string GetRoleColor(string role) => role switch
    {
        "Admin" => "#DC2626",
        "SalesAgent" => "#2563EB",
        "Customer" => "#10B981",
        _ => "#6B7280"
    };

    private static string GetRoleBgColor(string role) => role switch
    {
        "Admin" => "#FEE2E2",
        "SalesAgent" => "#DBEAFE",
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
                    // Create user via facade (saves to JSON)
                    var createResult = await _userFacade.CreateUserAsync(
                        username: dialog.ViewModel.Email.Split('@')[0], // Generate username from email
                        email: dialog.ViewModel.Email,
                        phoneNumber: dialog.ViewModel.Phone,
                        password: dialog.ViewModel.Password,
                        role: dialog.ViewModel.SelectedRole
                    );

                    if (createResult.IsSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] New user added: {dialog.ViewModel.FullName}");
                        await RefreshAsync();
                    }
                    else
                    {
                        await _toastHelper?.ShowError(createResult.ErrorMessage ?? "Failed to create user");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Error saving user: {ex.Message}");
                    await _toastHelper?.ShowError($"Error creating user: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Error adding user: {ex.Message}");
            await _toastHelper?.ShowError($"Error: {ex.Message}");
        }
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

    /// <summary>
    /// Show user details (fetches from API: GET /api/v1/users/{id})
    /// Uses IsDetailsLoading instead of IsLoading to avoid layout jumps from UserListSkeleton
    /// </summary>
    [RelayCommand]
    private async Task ViewUserDetailsAsync(UserViewModel user)
    {
        try
        {
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] ViewUserDetailsAsync - user is null");
                await _toastHelper?.ShowError("User not found");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] ViewUserDetailsAsync called for user: {user.Name} (ID: {user.Id})");
            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Items count before API call: {Items.Count}");
            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Calling API: GET /api/v1/users/{user.Id}");

            // Use IsDetailsLoading instead of IsLoading to avoid showing UserListSkeleton
            IsDetailsLoading = true;

            // Call API to get user details
            var result = await _userFacade.GetUserByIdAsync(user.Id);

            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] API response received, Items count after API: {Items.Count}");

            if (result.IsSuccess && result.Data != null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] API response received - User: {result.Data.Username}");
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Response data: Full Name={result.Data.FullName}, Email={result.Data.Email}, Phone={result.Data.PhoneNumber}");

                // Stop loading indicator before showing dialog
                // (ShowUserDetailsDialogAsync will await until dialog is closed)
                IsDetailsLoading = false;

                // Show user details dialog with API response data
                await _navigationService.ShowUserDetailsDialogAsync(result.Data);

                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] User details dialog displayed successfully: {result.Data.Username}");
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Items count after dialog display: {Items.Count}");
            }
            else
            {
                var errorMsg = result.ErrorMessage ?? "Failed to retrieve user details";
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] API call failed: {errorMsg}");
                IsDetailsLoading = false;
                await _toastHelper?.ShowError(errorMsg);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Error viewing user details: {ex.Message}\n{ex.StackTrace}");
            IsDetailsLoading = false;
            await _toastHelper?.ShowError($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete user (calls API: DELETE /api/v1/users/{id})
    /// </summary>
    [RelayCommand]
    private async Task DeleteUserAsync(UserViewModel user)
    {
        try
        {
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] DeleteUserAsync - user is null");
                await _toastHelper?.ShowError("User not found");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] DeleteUserAsync called for user: {user.Name} (ID: {user.Id})");
            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Calling API: DELETE /api/v1/users/{user.Id}");

            SetLoadingState(true);

            // Call API to delete user
            var deleteResult = await _userFacade.DeleteUserAsync(user.Id);

            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] DeleteUserAsync - Delete result received: IsSuccess={deleteResult.IsSuccess}");

            if (deleteResult.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] User deleted successfully: {user.Name}");
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] API response: Status=Success, Message={deleteResult.ErrorMessage}");

                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] About to show success toast");
                await _toastHelper?.ShowSuccess($"User '{user.Name}' deleted successfully");
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Success toast shown");

                // Reload the page to refresh the list
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Reloading user list...");
                CurrentPage = 1;
                await LoadPageAsync();
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] User list reloaded");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Delete failed: {deleteResult.ErrorMessage}");
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] API response: Status=Failed, Message={deleteResult.ErrorMessage}");
                await _toastHelper?.ShowError(deleteResult.ErrorMessage ?? "Failed to delete user");
            }

            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] DeleteUserAsync - About to call SetLoadingState(false)");
            SetLoadingState(false);
            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] DeleteUserAsync - SetLoadingState(false) called");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Error deleting user: {ex.Message}\n{ex.StackTrace}");
            await _toastHelper?.ShowError($"Error: {ex.Message}");
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
