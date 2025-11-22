namespace MyShop.Shared.Constants;

/// <summary>
/// Centralized user-facing messages for success and error scenarios
/// </summary>
public static class Messages
{
    // Auth Messages
    public const string LoginSuccess = "Welcome back!";
    public const string LoginFailed = "Invalid username or password";
    public const string LogoutSuccess = "You have been logged out";
    public const string RegisterSuccess = "Account created successfully";
    public const string RegisterFailed = "Failed to create account";
    
    // Network Errors
    public const string NetworkError = "Cannot connect to server. Please check your network connection.";
    public const string ServerError = "Server error occurred. Please try again later.";
    public const string Timeout = "Request timeout. Please try again.";
    
    // Validation Messages
    public const string RequiredField = "This field is required";
    public const string InvalidEmail = "Invalid email address";
    public const string InvalidPhone = "Invalid phone number";
    public const string PasswordTooShort = "Password must be at least 6 characters";
    public const string PasswordMismatch = "Passwords do not match";
    
    // Generic Messages
    public const string SaveSuccess = "Changes saved successfully";
    public const string SaveFailed = "Failed to save changes";
    public const string DeleteSuccess = "Item deleted successfully";
    public const string DeleteFailed = "Failed to delete item";
    public const string UpdateSuccess = "Item updated successfully";
    public const string UpdateFailed = "Failed to update item";
    public const string LoadFailed = "Failed to load data";
    
    // Trial & Activation
    public const string TrialActivated = "Trial activated successfully";
    public const string TrialExpired = "Your trial has expired";
    public const string EmailVerificationSent = "Verification email sent";
    public const string EmailVerified = "Email verified successfully";
}
