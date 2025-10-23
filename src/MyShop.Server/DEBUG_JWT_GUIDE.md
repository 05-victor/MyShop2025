# Debug JWT Token Claims - Troubleshooting Guide

## ?? Problem

API endpoint `/api/v1/auth/me` returns error:
```json
{
  "code": 404,
  "message": "User not found or invalid token",
  "result": null,
  "success": false
}
```

With log message:
```
warn: MyShop.Server.Services.Implementations.AuthService[0]
      User ID not found in JWT token
```

## ?? Root Cause

JWT token claims có th? có nhi?u tên khác nhau tùy thu?c vào c?u hình `MapInboundClaims`:

### When `MapInboundClaims = false`:
```json
{
  "nameid": "user-guid",        // User ID
  "unique_name": "username",     // Username
  "email": "user@example.com"    // Email
}
```

### When `MapInboundClaims = true` (default):
```json
{
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "user-guid",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "username",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "user@example.com"
}
```

## ? Solution Applied

### 1. Updated InfoJwtService.GetUserId()

Gi? nó s? th? **t?t c? các claim types** có th?:

```csharp
public static Guid? GetUserId(ClaimsPrincipal user)
{
    // Try ClaimTypes.NameIdentifier (long URI format)
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Try "sub" (JWT standard)
    if (string.IsNullOrEmpty(userIdClaim))
    {
        userIdClaim = user.FindFirst("sub")?.Value;
    }
    
    // Try "nameid" (when MapInboundClaims = false)
    if (string.IsNullOrEmpty(userIdClaim))
    {
        userIdClaim = user.FindFirst("nameid")?.Value;
    }
    
    return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
}
```

### 2. Added Debug Logging

**In Program.cs** - Log claims when token validated:
```csharp
OnTokenValidated = context =>
{
    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("? Token validated for user: {User}", context.Principal?.Identity?.Name);
    
    // Log all claims
    if (context.Principal != null)
    {
        logger.LogInformation("?? Token claims:");
        foreach (var claim in context.Principal.Claims)
        {
            logger.LogInformation("  - {Type}: {Value}", claim.Type, claim.Value);
        }
    }
    
    return Task.CompletedTask;
}
```

**In AuthService.GetMeAsync()** - Log claims when extracting:
```csharp
// Log all claims for debugging
_logger.LogInformation("?? All claims in token:");
foreach (var claim in httpContext.User.Claims)
{
    _logger.LogInformation("  - {Type}: {Value}", claim.Type, claim.Value);
}
```

## ?? How to Debug

### Step 1: Login và l?y token
```bash
POST http://localhost:5228/api/v1/auth/login
Content-Type: application/json

{
  "usernameOrEmail": "test@example.com",
  "password": "password123"
}
```

### Step 2: Copy token t? response
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."  // ? Copy this
  }
}
```

### Step 3: G?i Get Me v?i token
```bash
GET http://localhost:5228/api/v1/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Step 4: Xem logs trong Console
```
info: Program[0]
      ? Token validated for user: test
info: Program[0]
      ?? Token claims:
info: Program[0]
        - http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier: 3fa85f64-5717-4562-b3fc-2c963f66afa6
info: Program[0]
        - http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name: test
info: Program[0]
        - http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress: test@example.com

info: MyShop.Server.Services.Implementations.AuthService[0]
      ?? All claims in token:
info: MyShop.Server.Services.Implementations.AuthService[0]
        - http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier: 3fa85f64-5717-4562-b3fc-2c963f66afa6

info: MyShop.Server.Services.Implementations.AuthService[0]
      ? GetMe called from JWT token for userId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
```

## ?? Common Issues

### Issue 1: Token Expired
**Symptom:**
```
Authentication failed: IDX10223: Lifetime validation failed. The token is expired.
```

**Solution:** Token m?c ??nh expire sau 5 phút. Login l?i ?? l?y token m?i.

---

### Issue 2: Invalid Token Format
**Symptom:**
```
Authentication failed: IDX12709: The token is not valid.
```

**Solution:** 
- Ki?m tra Authorization header format: `Bearer <token>`
- Không có space th?a
- Không có quotes xung quanh token

---

### Issue 3: Wrong Claim Type
**Symptom:**
```
User ID not found in JWT token
```

**Solution:** 
- Xem logs ?? bi?t claim type th?c t?
- InfoJwtService.GetUserId() ?ã ???c c?p nh?t ?? th? t?t c? claim types

---

### Issue 4: MapInboundClaims Setting
**In Program.cs:**
```csharp
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false; // ? This affects claim names
    // ...
})
```

**Effect:**
- `false`: Claims use short names (nameid, unique_name, email)
- `true`: Claims use long URI format (http://schemas.xmlsoap.org/...)

**Our solution handles both!** ?

## ?? Verification Checklist

? Token is generated successfully (check login response)  
? Token is sent in Authorization header with "Bearer " prefix  
? Token is not expired (< 5 minutes old)  
? Claims are logged in console (see Step 4 above)  
? User ID claim exists in token  
? InfoJwtService.GetUserId() tries multiple claim types  

## ?? Expected Result

After fixes, you should see:

**Console Logs:**
```
info: MyShop.Server.Services.Implementations.AuthService[0]
      ? GetMe called from JWT token for userId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**API Response:**
```json
{
  "success": true,
  "message": "User profile retrieved successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "username": "test",
    "email": "test@example.com",
    "phoneNumber": "0123456789",
    "createdAt": "2025-01-18T02:00:00Z",
    "avatar": null,
    "activateTrial": true,
    "isVerified": false,
    "updatedAt": null,
    "roleNames": ["User"]
  },
  "statusCode": 200
}
```

## ?? Test Files

- **Debug Test**: `src/MyShop.Server/Controllers/debug-jwt-claims.http`
- **Normal Test**: `src/MyShop.Server/Controllers/auth-me-tests.http`

## ?? Next Steps

1. **Restart server** ?? apply changes
2. **Login l?i** ?? l?y token m?i
3. **Test Get Me** v?i token m?i
4. **Xem logs** ?? verify claims
5. **Xóa debug logs** sau khi fix xong (optional)

## ?? What We Learned

- JWT claim types có th? khác nhau tùy thu?c vào configuration
- Always support multiple claim type formats for compatibility
- Logging is crucial for debugging JWT issues
- MapInboundClaims setting affects claim names significantly