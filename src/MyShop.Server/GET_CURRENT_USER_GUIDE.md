# Get Current User Info - Implementation Guide

## ?? Overview

API `/api/v1/auth/me` ?ã ???c c?i thi?n ?? **t? ??ng l?y thông tin user t? JWT token** mà không c?n truy?n userId trong request. S? d?ng `InfoJwtService` ?? extract thông tin t? token m?t cách clean và maintainable.

## ? Improvements Made

### 1. **New Interface Method**
```csharp
// IAuthService.cs
Task<UserInfoResponse?> GetMeAsync();  // M?i - Không c?n userId
Task<UserInfoResponse?> GetMeAsync(Guid userId);  // Gi? l?i cho backward compatibility
```

### 2. **AuthService Implementation with InfoJwtService**
```csharp
public async Task<UserInfoResponse?> GetMeAsync()
{
    // S? d?ng InfoJwtService ?? extract userId t? JWT token
    var httpContext = _httpContextAccessor.HttpContext;
    var userId = InfoJwtService.GetUserId(httpContext.User);
    
    if (userId != null)
    {
        return await GetMeAsync(userId.Value);
    }
    
    return null;
}
```

### 3. **InfoJwtService - Clean Token Information Extraction**
```csharp
public class InfoJwtService
{
    // Extract User ID
    public static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
    
    // Extract Username
    public static string? GetUsername(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value;
    }
    
    // Extract Email
    public static string? GetEmail(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value;
    }
    
    // Extract Roles
    public static List<string> GetRoles(ClaimsPrincipal user)
    {
        return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }
    
    // Extract Authorities
    public static List<string> GetAuthorities(ClaimsPrincipal user)
    {
        return user.FindAll("authority").Select(c => c.Value).ToList();
    }
    
    // Check if user has specific authority
    public static bool HasAuthority(ClaimsPrincipal user, string authority)
    {
        return user.HasClaim("authority", authority);
    }
}
```

### 4. **Simplified Controller**
```csharp
[HttpGet("me")]
[Authorize]
public async Task<ActionResult<ApiResponse<UserInfoResponse>>> GetMe()
{
    var user = await _authService.GetMeAsync();  // ??n gi?n h?n!
    
    if (user == null)
        return NotFound(...);
    
    return Ok(...);
}
```

## ?? How to Use

### Step 1: Login ?? l?y JWT Token
```bash
POST /api/v1/auth/login
Content-Type: application/json

{
  "usernameOrEmail": "user@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "...",
    "username": "user",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."  // Copy this!
  }
}
```

### Step 2: G?i Get Me v?i Token
```bash
GET /api/v1/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**
```json
{
  "success": true,
  "message": "User profile retrieved successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "username": "user",
    "email": "user@example.com",
    "phoneNumber": "0123456789",
    "createdAt": "2025-01-18T00:00:00Z",
    "avatar": "https://...",
    "activateTrial": true,
    "isVerified": false,
    "updatedAt": null,
    "roleNames": ["User", "SalesAgent"]
  },
  "statusCode": 200
}
```

## ?? Key Features

? **Automatic User Detection** - Không c?n truy?n userId  
? **JWT Token Based** - L?y thông tin t? Bearer token  
? **Secure** - Yêu c?u authentication  
? **Clean Code** - Controller code ??n gi?n h?n  
? **Backward Compatible** - Method c? v?n ho?t ??ng  

## ?? Technical Details

### JWT Token Claims
Token ch?a các claims sau:
```csharp
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  // UserId
new Claim(ClaimTypes.Name, user.Username),
new Claim(ClaimTypes.Email, user.Email),
new Claim(ClaimTypes.Role, role),  // Multiple roles
new Claim("authority", authority)  // Multiple authorities
```

### HttpContextAccessor
```csharp
// Program.cs - DI Registration
builder.Services.AddHttpContextAccessor();

// AuthService - Constructor Injection
public AuthService(..., IHttpContextAccessor httpContextAccessor)
{
    _httpContextAccessor = httpContextAccessor;
}
```

## ?? Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | User unique identifier |
| `username` | string | Username |
| `email` | string | Email address |
| `phoneNumber` | string | Phone number (from Profile) |
| `createdAt` | DateTime | Account creation date |
| `avatar` | string? | Avatar URL (nullable) |
| `activateTrial` | bool | Trial status |
| `isVerified` | bool | Email verification status |
| `updatedAt` | DateTime? | Last update time (nullable) |
| `roleNames` | string[] | List of assigned roles |

## ?? Error Scenarios

### 1. No Token Provided
```http
GET /api/v1/auth/me
```
**Response:** `401 Unauthorized`
```json
{
  "success": false,
  "message": "Unauthorized",
  "statusCode": 401
}
```

### 2. Invalid Token
```http
GET /api/v1/auth/me
Authorization: Bearer invalid_token
```
**Response:** `401 Unauthorized`

### 3. Expired Token
```http
GET /api/v1/auth/me
Authorization: Bearer expired_token
```
**Response:** `401 Unauthorized`

### 4. User Not Found
Token valid nh?ng user b? xóa kh?i database
**Response:** `404 Not Found`
```json
{
  "success": false,
  "message": "User not found or invalid token",
  "statusCode": 404
}
```

## ?? Configuration

### JWT Token Expiry
```json
// appsettings.json
{
  "JwtSettings": {
    "ExpiryInMinutes": 5,  // Token expires after 5 minutes
    "RefreshTokenExpiryInDays": 1
  }
}
```

## ?? Testing

S? d?ng file test: `auth-me-tests.http`

```bash
# 1. Register user
POST /api/v1/auth/register

# 2. Login to get token
POST /api/v1/auth/login

# 3. Get user info with token
GET /api/v1/auth/me
Authorization: Bearer <token_from_step_2>
```

## ?? Benefits vs Old Approach

### Old Approach ?
```csharp
// Controller ph?i extract userId t? claims
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
if (Guid.TryParse(userIdClaim, out var userId))
{
    var user = await _authService.GetMeAsync(userId);
}
```

### New Approach ?
```csharp
// Controller ??n gi?n, logic ? Service layer
var user = await _authService.GetMeAsync();
```

## ?? Best Practices

1. ? **Always validate token** - S? d?ng `[Authorize]` attribute
2. ? **Handle null responses** - User có th? không t?n t?i
3. ? **Log important actions** - Track user profile access
4. ? **Return appropriate status codes** - 401, 404, 500
5. ? **Keep sensitive data secure** - Không return password hash

## ?? Related APIs

- `POST /api/v1/auth/login` - Get JWT token
- `POST /api/v1/auth/register` - Create new user
- `GET /api/v1/users/{userId}` - Get user by ID (admin only)

## ?? References

- JWT Token Documentation: `JwtService.cs`
- User Repository: `IUserRepository.cs`
- Authentication Flow: `AuthController.cs`