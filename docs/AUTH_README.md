# MyShop - Authentication Implementation

## ✅ Implementation Status

All authentication features use **REAL API calls** to the backend server.

### Endpoints
- `POST /api/v1/auth/register` - Register new user ✅
- `POST /api/v1/auth/login` - User login ✅
- `GET /api/v1/auth/me` - Get current user profile ✅

### Configuration
**API Base URL:** `src/MyShop.Client/ApiServer/ApiConfig.json`
```json
{
  "BaseUrl": "https://localhost:5228"
}
```

### Response Validation
```csharp
if (response.Code >= 200 && response.Code < 300 && response.Success) {
    // Success
}
```

### Features Status
- ✅ Register with email/password
- ✅ Login with email/password
- ✅ JWT token storage (Windows Credential Locker)
- ✅ Dashboard with user data
- ⚠️ Google OAuth (not implemented)
- ⚠️ Forgot Password (not implemented)

## Server Logs
Connection logging enabled:
```
✅ Client connected: {ConnectionId} from {IP}
⚠️ Client disconnected: {ConnectionId} from {IP}
```

---

## 🐛 Issues Fixed & Solutions

### 1. **404 Not Found Error** ❌ → ✅
**Problem:**
```
Response status code does not indicate success: 404 (Not found)
```

**Root Cause:**
- Client API routes didn't match server routes
- IAuthApi used `/auth/login` instead of `/api/v1/auth/login`

**Solution:**
```csharp
// ❌ Before (Wrong)
[Post("/auth/login")]

// ✅ After (Correct)
[Post("/api/v1/auth/login")]
```

**Files Changed:**
- `src/MyShop.Client/ApiServer/IAuthApi.cs`

**Lesson:** Always verify route matching between client and server controllers.

---

### 2. **Field Mapping Mismatch (PhoneNumber ↔ Sdt)** ❌ → ✅
**Problem:**
- Frontend ViewModel used `PhoneNumber`
- Backend DTO expected `Sdt` (Số điện thoại in Vietnamese)

**Solution:**
```csharp
// In RegisterViewModel
var request = new CreateUserRequest {
    Username = Username,
    Email = Email,
    Password = Password,
    Sdt = PhoneNumber,  // ✅ Map PhoneNumber → Sdt
    ActivateTrial = false,
    RoleNames = new() { }
};
```

**Files Changed:**
- `src/MyShop.Client/ViewModels/RegisterViewModel.cs`

**Lesson:** Check DTO field names carefully - backend may use different naming conventions.

---

### 3. **LoginResponse Missing Fields** ❌ → ✅
**Problem:**
- Backend returns `PhoneNumber`, `Avatar`, `ActivateTrial`, `IsVerified`
- Frontend DTO didn't have these fields
- Dashboard couldn't display user info correctly

**Solution:**
Updated `LoginResponse.cs` and `CreateUserResponse.cs`:
```csharp
public class LoginResponse {
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }      // ✅ Added
    public string? Avatar { get; set; }           // ✅ Added
    public bool ActivateTrial { get; set; }       // ✅ Added
    public bool IsVerified { get; set; }          // ✅ Added
    public List<string> RoleNames { get; set; }
    public string Token { get; set; }
}
```

**Files Changed:**
- `src/MyShop.Shared/DTOs/Responses/LoginResponse.cs`
- `src/MyShop.Shared/DTOs/Responses/CreateUserResponse.cs`

**Lesson:** Keep DTOs in sync between client and server - especially when backend adds new fields.

---

### 4. **Backend Not Returning Profile Data** ❌ → ✅
**Problem:**
- `AuthService.LoginAsync()` didn't include `Profile` navigation property
- Response had null values for `PhoneNumber` and `Avatar`

**Solution:**
Updated `UserRepository` to include Profile in all queries:
```csharp
public async Task<User?> GetByUsernameAsync(string username) {
    return await _context.Users
        .Include(u => u.Roles)
        .Include(u => u.Profile)  // ✅ Added
        .FirstOrDefaultAsync(u => u.Username == username);
}
```

Updated `AuthService` to map Profile fields:
```csharp
return new LoginResponse {
    Id = user.Id,
    Username = user.Username,
    Email = user.Email,
    PhoneNumber = user.Profile?.PhoneNumber,  // ✅ Map from Profile
    Avatar = user.Profile?.Avatar,            // ✅ Map from Profile
    ActivateTrial = user.IsTrialActive,
    IsVerified = user.IsEmailVerified,
    RoleNames = user.Roles.Select(r => r.Name).ToList(),
    Token = token
};
```

**Files Changed:**
- `src/MyShop.Data/Repositories/UserRepository.cs`
- `src/MyShop.Server/Services/Implementations/AuthService.cs`

**Lesson:** Always include navigation properties when Entity Framework needs to load related data.

---

### 5. **XAML Compile Error (RegisterView.xaml)** ❌ → ✅
**Problem:**
```
CS1061: 'RegisterView' does not contain a definition for 'InitializeComponent'
```

**Root Cause:**
- `.csproj` had `<Page Remove="Views\RegisterView.xaml" />`
- XAML compiler couldn't generate `RegisterView.g.cs`

**Solution:**
Removed the exclusion from `MyShop.Client.csproj`:
```xml
<!-- ❌ Removed this line -->
<Page Remove="Views\RegisterView.xaml" />
```

**Files Changed:**
- `src/MyShop.Client/MyShop.Client.csproj`

**Lesson:** Check `.csproj` for `<Page Remove>` or `<Compile Remove>` if getting InitializeComponent errors.

---

### 6. **Server Connection Logging** ✅
**Requirement:**
- Log when client connects: `✅ Client connected`
- Log when client disconnects: `⚠️ Client disconnected`

**Solution:**
Added middleware in `Program.cs`:
```csharp
app.Use(async (context, next) => {
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var connectionId = context.Connection.Id;
    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    
    logger.LogInformation("✅ Client connected: {ConnectionId} from {ClientIP}", 
        connectionId, clientIp);
    
    try {
        await next();
    }
    finally {
        logger.LogInformation("⚠️ Client disconnected: {ConnectionId} from {ClientIP}", 
            connectionId, clientIp);
    }
});
```

**Files Changed:**
- `src/MyShop.Server/Program.cs`

**Lesson:** Middleware runs for every request - useful for logging, authentication, etc.

---

## 📋 Important Notes & Best Practices

### 1. **Field Validation - Individual vs Grouped**
**Current Implementation:**
- Each input field shows its own error message
- Error messages appear below each field
- Uses `ObservableValidator` base class

**How it works:**
```csharp
[ObservableProperty]
[Required(ErrorMessage = "Username is required")]
private string _username = string.Empty;

[ObservableProperty]
private string _usernameError = string.Empty;

private void UpdateFieldErrors() {
    UsernameError = GetErrors(nameof(Username))
        .FirstOrDefault()?.ErrorMessage ?? string.Empty;
}
```

### 2. **Response Validation Pattern**
**Always use this pattern:**
```csharp
if (response.Code >= 200 && response.Code < 300 && response.Success) {
    // Success - proceed
}
else {
    // Error - show response.Message
}
```

**Why both checks?**
- `Code >= 200 && Code < 300`: HTTP status success
- `Success == true`: Business logic success
- Both must pass for valid response

### 3. **JWT Token Storage**
**Using Windows Credential Locker:**
```csharp
// Save
CredentialHelper.SaveToken(token);

// Retrieve
var token = CredentialHelper.GetToken();

// Remove
CredentialHelper.RemoveToken();
```

**Location:** Encrypted in Windows Credential Manager
**Security:** ✅ Secure, ✅ Encrypted, ✅ Per-user

### 4. **API Base URL Configuration**
**Development:**
```json
{ "BaseUrl": "https://localhost:7120" }
```

**Production:**
```json
{ "BaseUrl": "https://api.myshop.com" }
```

**File:** `src/MyShop.Client/ApiServer/ApiConfig.json`

### 5. **Error Handling Strategy**
```csharp
try {
    var response = await _authApi.LoginAsync(request);
    // Handle success
}
catch (ApiException ex) when (ex.Content != null) {
    // Parse API error response
    var errorContent = await ex.GetContentAsAsync<ApiResponse<object>>();
    ErrorMessage = errorContent?.Message ?? ex.Message;
}
catch (Exception ex) {
    // Handle unexpected errors
    ErrorMessage = $"Unexpected error: {ex.Message}";
}
finally {
    IsLoading = false;
}
```

### 6. **Navigation with Data**
**Pass data to next view:**
```csharp
_navigationService.NavigateTo(typeof(DashboardView), userData);
```

**Receive data in view:**
```csharp
protected override void OnNavigatedTo(NavigationEventArgs e) {
    if (e.Parameter is LoginResponse userData) {
        ViewModel.Initialize(userData);
    }
}
```

---

## ⚠️ Known Limitations & TODOs

### 1. **Google OAuth** (Not Implemented)
**Current:** Shows placeholder message
**TODO:**
- Implement OAuth2 flow
- Add Google Client ID configuration
- Create backend endpoint `/api/v1/auth/google-login`
- Handle token exchange

### 2. **Forgot Password** (Not Implemented)
**Current:** Shows placeholder message
**TODO:**
- Create password reset flow
- Add email service integration
- Create reset token validation
- Add reset password page

### 3. **Email Verification** (Partially Implemented)
**Current:** `IsVerified` field exists but no verification flow
**TODO:**
- Send verification email on registration
- Create email verification endpoint
- Add verification token validation
- Update UI to show verification status

### 4. **Trial Activation** (Logic Exists, UI Pending)
**Current:** Backend tracks trial status
**TODO:**
- Add trial activation UI
- Show trial expiration date
- Restrict features for non-trial users
- Create upgrade/payment flow

### 5. **Role-Based UI** (Partially Implemented)
**Current:** Dashboard checks `IsAdmin`
**TODO:**
- Implement permission checking for all features
- Hide/disable UI elements based on roles
- Show "feature locked" dialogs
- Create admin-only pages

---

## 🔧 Troubleshooting Guide

### Issue: 404 Not Found
**Check:**
1. API routes match in `IAuthApi` and `AuthController`
2. Server is running on correct port (7120)
3. `ApiConfig.json` has correct base URL

### Issue: Validation Errors Not Showing
**Check:**
1. ViewModel inherits from `ObservableValidator`
2. Properties have `[Required]` or validation attributes
3. `ValidateAllProperties()` is called before submit
4. Error properties are bound in XAML

### Issue: Token Not Persisting
**Check:**
1. `CredentialHelper.SaveToken()` is called after login
2. Token is retrieved on app startup
3. Windows Credential Manager has entry for "MyShopJwtToken"

### Issue: Profile Data is Null
**Check:**
1. `UserRepository` includes `.Include(u => u.Profile)`
2. `AuthService` maps Profile fields to response
3. Profile entity exists in database for the user

### Issue: Server Not Logging Connections
**Check:**
1. Middleware is registered in `Program.cs`
2. Middleware is placed BEFORE `UseHttpsRedirection()`
3. Logging level is set to `Information` or lower

---

## 📚 References

### Related Documentation
- `docs/REMOVED_AUTHORITIES_IMPLEMENTATION.md` - Role & permission system
- `docs/REMOVED_AUTHORITIES_QUICK_REF.md` - Quick reference for authorities
- `src/MyShop.Server/Controllers/AUTH_API_DOCUMENTATION.md` - Old API docs (deprecated)

### Key Files
**Client:**
- `src/MyShop.Client/ViewModels/LoginViewModel.cs`
- `src/MyShop.Client/ViewModels/RegisterViewModel.cs`
- `src/MyShop.Client/ViewModels/DashboardViewModel.cs`
- `src/MyShop.Client/ApiServer/IAuthApi.cs`
- `src/MyShop.Client/Helpers/CredentialHelper.cs`

**Server:**
- `src/MyShop.Server/Controllers/AuthController.cs`
- `src/MyShop.Server/Services/Implementations/AuthService.cs`
- `src/MyShop.Data/Repositories/UserRepository.cs`

**Shared:**
- `src/MyShop.Shared/DTOs/Requests/LoginRequest.cs`
- `src/MyShop.Shared/DTOs/Requests/CreateUserRequest.cs`
- `src/MyShop.Shared/DTOs/Responses/LoginResponse.cs`
- `src/MyShop.Shared/DTOs/Common/ApiResponse.cs`

---

## 🎯 Summary

**What Works:** ✅
- Registration with email/password
- Login with email/password
- JWT token authentication
- Token storage in Windows Credential Locker
- Profile data (phone, avatar)
- Role checking (Admin vs User)
- Trial status tracking
- Email verification status
- Server connection logging

**What's Pending:** ⚠️
- Google OAuth integration
- Password recovery flow
- Email verification flow
- Trial activation UI
- Complete role-based UI restrictions

**Last Updated:** 2025-01-12
