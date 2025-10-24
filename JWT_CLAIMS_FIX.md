# JWT Claims Issue - Fixed

## Problem Summary

The issue was that `CurrentUserService` was failing to read the user ID from JWT claims because of a mismatch between how claims are created and how they're being read.

## Root Cause

When `MapInboundClaims = false` is set in JWT Bearer configuration (Program.cs line 53), the claim type names change:

| Claim Creation (JwtService) | Claim Type in Token | What CurrentUserService Was Looking For |
|---------------------------|---------------------|----------------------------------------|
| `ClaimTypes.NameIdentifier` | `"nameid"` | `ClaimTypes.NameIdentifier` (doesn't match!) |
| `ClaimTypes.Name` | `"unique_name"` | `ClaimTypes.Name` (doesn't match!) |
| `ClaimTypes.Email` | `"email"` | `ClaimTypes.Email` (matches) |

## The Fix

Updated `CurrentUserService.cs` to check for **multiple claim types** in order:

### UserId Property
```csharp
public Guid? UserId
{
    get
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return null;

        // Try multiple claim types to support both MapInboundClaims = true and false
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value  // Standard .NET claim
                       ?? user.FindFirst("sub")?.Value                       // JWT standard claim
                       ?? user.FindFirst("nameid")?.Value;                   // When MapInboundClaims = false

        return Guid.TryParse(userIdClaim, out var id) ? id : null;
    }
}
```

### Username Property
```csharp
public string? Username
{
    get
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return null;

        // Try multiple claim types
        return user.FindFirst(ClaimTypes.Name)?.Value          // Standard .NET claim
            ?? user.FindFirst("unique_name")?.Value           // When MapInboundClaims = false
            ?? user.Identity?.Name;                           // Fallback
    }
}
```

### Email Property
```csharp
public string? Email
{
    get
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return null;

        // Try multiple claim types
        return user.FindFirst(ClaimTypes.Email)?.Value         // Standard .NET claim
            ?? user.FindFirst("email")?.Value;                // JWT standard claim
    }
}
```

## Testing

### 1. Login to get a token
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "usernameOrEmail": "testuser",
  "password": "password123"
}
```

### 2. Check the token claims in logs
When `OnTokenValidated` fires, you should see logs like:
```
? Token validated for user: testuser
?? Token claims:
  - nameid: 12345678-1234-1234-1234-123456789012
  - unique_name: testuser
  - email: test@example.com
  - jti: ...
  - iat: ...
  - authority: ALL
  - http://schemas.microsoft.com/ws/2008/06/identity/claims/role: Admin
```

### 3. Call an endpoint that uses CurrentUserService
```http
POST /api/v1/users/activate
Authorization: Bearer <your-token>
Content-Type: application/json

{
  "activateCode": "TEST123"
}
```

This should now work correctly and `_currentUser.UserId` will have a value!

## Additional Changes Made

- Removed duplicate `AddHttpContextAccessor()` registration from `Program.cs`
- Cleaned up service registration order for better clarity
- Added comprehensive logging to `OnTokenValidated` event to debug claims

## Why This Solution Works

The updated `CurrentUserService` now tries multiple claim type names, making it compatible with:
1. ? `MapInboundClaims = false` (current configuration) - uses `"nameid"`, `"unique_name"`, `"email"`
2. ? `MapInboundClaims = true` (if you change it) - uses `ClaimTypes.*` constants
3. ? JWT standard claims - uses `"sub"`, `"email"`, etc.

This makes your service robust and flexible regardless of JWT configuration changes.

## Verification

After restarting your application:
1. Login to get a fresh JWT token
2. Check the logs to see the actual claim types in your token
3. Call the activate endpoint - it should now successfully read the userId
4. The error "Invalid JWT: userId claim is missing" should be gone!

## Reference Files Modified
- ? `src/MyShop.Server/Services/Implementations/CurrentUserService.cs`
- ? `src/MyShop.Server/Program.cs`