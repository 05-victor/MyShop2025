namespace MyShop.Shared.Constants;

/// <summary>
/// Input validation error messages
/// </summary>
public static class ValidationMessages
{
    // Username
    public const string UsernameRequired = "Username is required";
    public const string UsernameTooShort = "Username must be at least 3 characters";
    public const string UsernameTooLong = "Username cannot exceed 50 characters";
    public const string UsernameInvalidFormat = "Username can only contain letters, numbers, and underscores";
    
    // Email
    public const string EmailRequired = "Email is required";
    public const string EmailInvalid = "Invalid email format";
    
    // Password
    public const string PasswordRequired = "Password is required";
    public const string PasswordTooShort = "Password must be at least 6 characters";
    public const string PasswordTooLong = "Password cannot exceed 100 characters";
    public const string PasswordConfirmMismatch = "Passwords do not match";
    
    // Phone
    public const string PhoneInvalid = "Invalid phone number format";
    public const string PhoneTooShort = "Phone number must be at least 10 digits";
    
    // Product
    public const string ProductNameRequired = "Product name is required";
    public const string ProductPriceInvalid = "Price must be greater than 0";
    public const string ProductStockInvalid = "Stock must be 0 or greater";
    
    // Order
    public const string OrderItemsEmpty = "Order must contain at least one item";
    public const string OrderQuantityInvalid = "Quantity must be greater than 0";
}
