using MyShop.Shared.Models;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.Services;

/// <summary>
/// Service to cache and manage the currently logged-in user.
/// Provides access to user data throughout the application without repeated API calls.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Get the currently cached user.
    /// </summary>
    User? CurrentUser { get; }

    /// <summary>
    /// Set the current user (called after login or GetMe).
    /// </summary>
    void SetCurrentUser(User? user);

    /// <summary>
    /// Clear the current user (called on logout).
    /// </summary>
    void ClearCurrentUser();

    /// <summary>
    /// Event raised when current user changes.
    /// </summary>
    event EventHandler<UserChangedEventArgs>? CurrentUserChanged;
}

/// <summary>
/// Event args for current user change.
/// </summary>
public class UserChangedEventArgs : EventArgs
{
    public User? PreviousUser { get; set; }
    public User? CurrentUser { get; set; }
}

/// <summary>
/// Implementation of ICurrentUserService.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private User? _currentUser;

    public User? CurrentUser => _currentUser;

    public event EventHandler<UserChangedEventArgs>? CurrentUserChanged;

    public void SetCurrentUser(User? user)
    {
        var previousUser = _currentUser;
        _currentUser = user;

        // Raise event
        CurrentUserChanged?.Invoke(
            this,
            new UserChangedEventArgs { PreviousUser = previousUser, CurrentUser = user }
        );

        System.Diagnostics.Debug.WriteLine($"[CurrentUserService] CurrentUser cached:");
        if (user != null)
        {
            System.Diagnostics.Debug.WriteLine($"  - Username: {user.Username}");
            System.Diagnostics.Debug.WriteLine($"  - Email: {user.Email}");
            System.Diagnostics.Debug.WriteLine($"  - FullName: {user.FullName}");
            System.Diagnostics.Debug.WriteLine($"  - PhoneNumber: {user.PhoneNumber}");
            System.Diagnostics.Debug.WriteLine($"  - Address: {user.Address}");
            System.Diagnostics.Debug.WriteLine($"  - Avatar: {user.Avatar}");
            System.Diagnostics.Debug.WriteLine($"  - IsEmailVerified: {user.IsEmailVerified}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"  - User cleared (null)");
        }
    }

    public void ClearCurrentUser()
    {
        SetCurrentUser(null);
    }
}
