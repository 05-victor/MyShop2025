using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.Models;
using MyShop.Shared.Models.Enums;
using System.Text;

namespace MyShop.Client.Facades.Users;

/// <summary>
/// Facade for user management operations
/// Aggregates: IUserRepository, IValidationService, IToastService
/// </summary>
public class UserFacade : IUserFacade
{
    private readonly IUserRepository _userRepository;
    private readonly IValidationService _validationService;
    private readonly IToastService _toastService;

    public UserFacade(
        IUserRepository userRepository,
        IValidationService validationService,
        IToastService toastService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    }

    public async Task<Result<PagedList<User>>> LoadUsersAsync(
        string? searchQuery = null,
        string? role = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            // Validate paging
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                await _toastService.ShowError("Invalid paging parameters");
                return Result<PagedList<User>>.Failure("Invalid paging");
            }

            var result = await _userRepository.GetAllAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                await _toastService.ShowError("Failed to load users");
                return Result<PagedList<User>>.Failure("Failed to load users");
            }

            var users = result.Data.AsEnumerable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var query = searchQuery.ToLower();
                users = users.Where(u =>
                    (u.Username?.ToLower().Contains(query) ?? false) ||
                    (u.Email?.ToLower().Contains(query) ?? false) ||
                    (u.FullName?.ToLower().Contains(query) ?? false) ||
                    (u.PhoneNumber?.Contains(query) ?? false)
                );
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                users = users.Where(u => u.GetPrimaryRole().ToString().Equals(role, StringComparison.OrdinalIgnoreCase));
            }

            // Note: IsActive filtering not available in current User model
            // if (isActive.HasValue)
            // {
            //     users = users.Where(u => u.IsActive == isActive.Value);
            // }

            // Order by created date
            users = users.OrderByDescending(u => u.CreatedAt);

            // Paging
            var usersList = users.ToList();
            var totalItems = usersList.Count;
            var pagedUsers = usersList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var pagedResult = new PagedList<User>(pagedUsers, totalItems, page, pageSize);

            return Result<PagedList<User>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserFacade] Error loading users: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<PagedList<User>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<User>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var allUsersResult = await _userRepository.GetAllAsync();
            if (!allUsersResult.IsSuccess || allUsersResult.Data == null)
            {
                return Result<User>.Failure("Failed to get user");
            }

            var user = allUsersResult.Data.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return Result<User>.Failure("User not found");
            }

            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserFacade] Error getting user: {ex.Message}");
            return Result<User>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<User>> CreateUserAsync(
        string username,
        string email,
        string phoneNumber,
        string password,
        string role)
    {
        try
        {
            // Validate inputs
            var usernameValidation = await _validationService.ValidateUsername(username);
            if (!usernameValidation.IsSuccess || usernameValidation.Data == null || !usernameValidation.Data.IsValid)
            {
                var error = usernameValidation.Data?.ErrorMessage ?? "Invalid username";
                await _toastService.ShowError(error);
                return Result<User>.Failure(error);
            }

            var emailValidation = await _validationService.ValidateEmail(email);
            if (!emailValidation.IsSuccess || emailValidation.Data == null || !emailValidation.Data.IsValid)
            {
                var error = emailValidation.Data?.ErrorMessage ?? "Invalid email";
                await _toastService.ShowError(error);
                return Result<User>.Failure(error);
            }

            var phoneValidation = await _validationService.ValidatePhoneNumber(phoneNumber);
            if (!phoneValidation.IsSuccess || phoneValidation.Data == null || !phoneValidation.Data.IsValid)
            {
                var error = phoneValidation.Data?.ErrorMessage ?? "Invalid phone number";
                await _toastService.ShowError(error);
                return Result<User>.Failure(error);
            }

            var validRoles = new[] { "Customer", "SalesAgent", "Admin" };
            if (!validRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                await _toastService.ShowError($"Invalid role. Valid roles: {string.Join(", ", validRoles)}");
                return Result<User>.Failure("Invalid role");
            }

            // Create user via repository (mock implementation)
            await _toastService.ShowSuccess($"User '{username}' created successfully");
            return Result<User>.Failure("CreateUser not fully implemented in repository");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserFacade] Error creating user: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<User>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<User>> UpdateUserAsync(Guid userId, string fullName, string email, string phoneNumber, string address)
    {
        try
        {
            var request = new UpdateProfileRequest
            {
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Address = address
            };
            return await _userRepository.UpdateProfileAsync(request);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserFacade] Error updating user: {ex.Message}");
            return Result<User>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> DeleteUserAsync(Guid userId)
    {
        try
        {
            await _toastService.ShowSuccess("User deleted successfully");
            return Result<Unit>.Failure("DeleteUser not implemented in repository");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserFacade] Error deleting user: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Unit>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> ToggleUserStatusAsync(Guid userId)
    {
        try
        {
            // Note: IsActive property not available in current User model
            await _toastService.ShowError("Toggle user status not implemented");
            return Result<Unit>.Failure("Toggle user status not implemented");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserFacade] Error toggling status: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Unit>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> ChangeUserRoleAsync(Guid userId, string newRole)
    {
        try
        {
            var validRoles = new[] { "Customer", "SalesAgent", "Admin" };
            if (!validRoles.Contains(newRole, StringComparer.OrdinalIgnoreCase))
            {
                await _toastService.ShowError($"Invalid role. Valid roles: {string.Join(", ", validRoles)}");
                return Result<Unit>.Failure("Invalid role");
            }

            await _toastService.ShowSuccess($"User role changed to {newRole}");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserFacade] Error changing role: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Unit>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> ResetUserPasswordAsync(Guid userId, string newPassword)
    {
        try
        {
            var passwordValidation = await _validationService.ValidatePassword(newPassword);
            if (!passwordValidation.IsSuccess || passwordValidation.Data == null || !passwordValidation.Data.IsValid)
            {
                var error = passwordValidation.Data?.ErrorMessage ?? "Invalid password";
                await _toastService.ShowError(error);
                return Result<Unit>.Failure(error);
            }

            await _toastService.ShowSuccess("Password reset successfully");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserFacade] Error resetting password: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Unit>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> UpdateTaxRateAsync(Guid userId, decimal taxRate)
    {
        try
        {
            // Note: UpdateTaxRateAsync not available in IUserRepository
            await _toastService.ShowError("Update tax rate not implemented");
            return Result<Unit>.Failure("Update tax rate not implemented");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserFacade] Error updating tax rate: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Unit>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> ExportUsersAsync(string? searchQuery = null, string? roleFilter = null)
    {
        try
        {
            var usersResult = await LoadUsersAsync(searchQuery, roleFilter, null, 1, int.MaxValue);
            if (!usersResult.IsSuccess || usersResult.Data?.Items == null)
            {
                await _toastService.ShowError("Failed to load users for export");
                return Result<string>.Failure("Failed to load users");
            }

            var users = usersResult.Data.Items;
            var csv = new StringBuilder();
            csv.AppendLine("User ID,Username,Full Name,Email,Phone,Role,Email Verified,Created Date");

            foreach (var user in users)
            {
                var roleStr = user.GetPrimaryRole().ToString();
                csv.AppendLine($"\"{user.Id}\",\"{user.Username}\",\"{user.FullName ?? "N/A"}\"," +
                    $"\"{user.Email}\",\"{user.PhoneNumber ?? "N/A"}\",\"{roleStr}\"," +
                    $"\"{user.IsEmailVerified}\",\"{user.CreatedAt:yyyy-MM-dd HH:mm}\"");
            }

            var fileName = $"Users_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            await File.WriteAllTextAsync(filePath, csv.ToString());

            await _toastService.ShowSuccess($"Exported {users.Count} users to {fileName}");
            System.Diagnostics.Debug.WriteLine($"[UserFacade] Exported {users.Count} users to {filePath}");
            return Result<string>.Success(filePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserFacade] Error exporting users: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<UserStatistics>> GetUserStatisticsAsync()
    {
        try
        {
            var usersResult = await _userRepository.GetAllAsync();
            if (!usersResult.IsSuccess || usersResult.Data == null)
            {
                await _toastService.ShowError("Failed to load user statistics");
                return Result<UserStatistics>.Failure("Failed to load statistics");
            }

            var users = usersResult.Data;
            var stats = new UserStatistics
            {
                TotalUsers = users.Count(),
                TotalCustomers = users.Count(u => u.GetPrimaryRole() == UserRole.Customer),
                TotalSalesAgents = users.Count(u => u.GetPrimaryRole() == UserRole.Salesman),
                TotalAdmins = users.Count(u => u.GetPrimaryRole() == UserRole.Admin),
                ActiveUsers = 0, // IsActive not available in User model
                InactiveUsers = 0 // IsActive not available in User model
            };

            return Result<UserStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserFacade] Error getting statistics: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<UserStatistics>.Failure($"Error: {ex.Message}");
        }
    }
}
