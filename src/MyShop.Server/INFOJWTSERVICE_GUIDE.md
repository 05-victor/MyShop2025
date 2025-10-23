# InfoJwtService - JWT Token Information Extractor

## ?? Overview

`InfoJwtService` là m?t utility service ??n gi?n ?? extract thông tin t? JWT token claims m?t cách clean và type-safe.

## ?? Purpose

Thay vì ph?i dùng tr?c ti?p:
```csharp
// ? Old way - Repetitive and error-prone
var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
if (Guid.TryParse(userIdClaim, out var userId))
{
    // use userId
}
```

Gi? có th? dùng:
```csharp
// ? New way - Clean and simple
var userId = InfoJwtService.GetUserId(user);
if (userId != null)
{
    // use userId.Value
}
```

## ?? Available Methods

### 1. **GetUserId(ClaimsPrincipal user)**
Extract User ID t? token
```csharp
public static Guid? GetUserId(ClaimsPrincipal user)
{
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
}
```

**Usage:**
```csharp
var userId = InfoJwtService.GetUserId(httpContext.User);
if (userId != null)
{
    Console.WriteLine($"User ID: {userId.Value}");
}
```

---

### 2. **GetUsername(ClaimsPrincipal user)**
Extract Username t? token
```csharp
public static string? GetUsername(ClaimsPrincipal user)
{
    return user.FindFirst(ClaimTypes.Name)?.Value;
}
```

**Usage:**
```csharp
var username = InfoJwtService.GetUsername(httpContext.User);
if (username != null)
{
    Console.WriteLine($"Username: {username}");
}
```

---

### 3. **GetEmail(ClaimsPrincipal user)**
Extract Email t? token
```csharp
public static string? GetEmail(ClaimsPrincipal user)
{
    return user.FindFirst(ClaimTypes.Email)?.Value;
}
```

**Usage:**
```csharp
var email = InfoJwtService.GetEmail(httpContext.User);
Console.WriteLine($"Email: {email}");
```

---

### 4. **GetRoles(ClaimsPrincipal user)**
Extract t?t c? Roles t? token
```csharp
public static List<string> GetRoles(ClaimsPrincipal user)
{
    return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
}
```

**Usage:**
```csharp
var roles = InfoJwtService.GetRoles(httpContext.User);
Console.WriteLine($"User has {roles.Count} roles: {string.Join(", ", roles)}");
```

---

### 5. **GetAuthorities(ClaimsPrincipal user)**
Extract t?t c? Authorities t? token
```csharp
public static List<string> GetAuthorities(ClaimsPrincipal user)
{
    return user.FindAll("authority").Select(c => c.Value).ToList();
}
```

**Usage:**
```csharp
var authorities = InfoJwtService.GetAuthorities(httpContext.User);
Console.WriteLine($"User has authorities: {string.Join(", ", authorities)}");
```

---

### 6. **HasAuthority(ClaimsPrincipal user, string authority)**
Check xem user có authority c? th? không
```csharp
public static bool HasAuthority(ClaimsPrincipal user, string authority)
{
    return user.HasClaim("authority", authority);
}
```

**Usage:**
```csharp
if (InfoJwtService.HasAuthority(httpContext.User, "DELETE"))
{
    Console.WriteLine("User can delete");
}
```

---

### 7. **GetJti(ClaimsPrincipal user)**
Extract JWT ID (JTI) t? token
```csharp
public static string? GetJti(ClaimsPrincipal user)
{
    return user.FindFirst("jti")?.Value;
}
```

**Usage:**
```csharp
var jti = InfoJwtService.GetJti(httpContext.User);
Console.WriteLine($"JWT ID: {jti}");
```

---

## ?? Real-World Usage Examples

### Example 1: In AuthService
```csharp
public async Task<UserInfoResponse?> GetMeAsync()
{
    var httpContext = _httpContextAccessor.HttpContext;
    if (httpContext == null) return null;

    // Use InfoJwtService to get user ID
    var userId = InfoJwtService.GetUserId(httpContext.User);
    
    if (userId == null)
    {
        _logger.LogWarning("User ID not found in token");
        return null;
    }

    return await GetMeAsync(userId.Value);
}
```

### Example 2: In Custom Authorization Handler
```csharp
public class CustomAuthorizationHandler : AuthorizationHandler<CustomRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomRequirement requirement)
    {
        // Get user info from token
        var userId = InfoJwtService.GetUserId(context.User);
        var username = InfoJwtService.GetUsername(context.User);
        var roles = InfoJwtService.GetRoles(context.User);
        var authorities = InfoJwtService.GetAuthorities(context.User);

        // Check permissions
        if (InfoJwtService.HasAuthority(context.User, "ALL"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### Example 3: In Controller Action
```csharp
[HttpGet("my-data")]
[Authorize]
public IActionResult GetMyData()
{
    // Extract all user info from token
    var userId = InfoJwtService.GetUserId(User);
    var username = InfoJwtService.GetUsername(User);
    var email = InfoJwtService.GetEmail(User);
    var roles = InfoJwtService.GetRoles(User);
    var authorities = InfoJwtService.GetAuthorities(User);

    return Ok(new
    {
        UserId = userId,
        Username = username,
        Email = email,
        Roles = roles,
        Authorities = authorities
    });
}
```

### Example 4: Authorization Check
```csharp
[HttpDelete("products/{id}")]
[Authorize]
public async Task<IActionResult> DeleteProduct(Guid id)
{
    // Check if user has DELETE authority
    if (!InfoJwtService.HasAuthority(User, "DELETE"))
    {
        return Forbid("You don't have permission to delete");
    }

    await _productService.DeleteAsync(id);
    return NoContent();
}
```

## ?? Benefits

? **Clean Code** - Không c?n repeat logic extract claims  
? **Type Safety** - Return ?úng type (Guid?, string?, List<string>)  
? **Null Safety** - Handle null cases properly  
? **Reusable** - Dùng ???c ? b?t k? ?âu c?n extract token info  
? **Testable** - D? dàng unit test  
? **Maintainable** - Logic t?p trung ? m?t ch?  

## ?? JWT Token Claims Structure

JWT token ch?a các claims sau:

```json
{
  "nameid": "3fa85f64-5717-4562-b3fc-2c963f66afa6",  // User ID
  "unique_name": "john_doe",                          // Username
  "email": "john@example.com",                        // Email
  "role": ["User", "Admin"],                          // Roles (multiple)
  "authority": ["POST", "DELETE", "ALL"],             // Authorities (multiple)
  "jti": "unique-token-id",                           // JWT ID
  "iat": 1234567890,                                  // Issued At
  "exp": 1234567890                                   // Expiration
}
```

## ?? Testing

### Unit Test Example
```csharp
[Fact]
public void GetUserId_ValidClaim_ReturnsGuid()
{
    // Arrange
    var userId = Guid.NewGuid();
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
    };
    var identity = new ClaimsIdentity(claims);
    var user = new ClaimsPrincipal(identity);

    // Act
    var result = InfoJwtService.GetUserId(user);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(userId, result.Value);
}

[Fact]
public void GetUserId_NoClaim_ReturnsNull()
{
    // Arrange
    var user = new ClaimsPrincipal();

    // Act
    var result = InfoJwtService.GetUserId(user);

    // Assert
    Assert.Null(result);
}
```

## ?? Related Files

- **Implementation**: `src/MyShop.Server/Services/Implementations/InfoJwtService.cs`
- **Usage in AuthService**: `src/MyShop.Server/Services/Implementations/AuthService.cs`
- **JWT Token Generation**: `src/MyShop.Server/Services/Implementations/JwtService.cs`

## ?? Additional Resources

- [ClaimsPrincipal Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimsprincipal)
- [JWT Claims Standard](https://datatracker.ietf.org/doc/html/rfc7519#section-4)
- [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)