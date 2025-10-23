# [Authorize] Attribute - Complete Guide

## ?? Khi nào c?n dùng [Authorize]?

### ? **C?N dùng [Authorize]** khi:

1. **Endpoint yêu c?u user ph?i ??ng nh?p**
   ```csharp
   [HttpGet("me")]
   [Authorize]  // ? User PH?I có valid JWT token
   public async Task<IActionResult> GetMe()
   {
       // Ch? authenticated users m?i access ???c
   }
   ```

2. **Endpoint ch?a thông tin nh?y c?m**
   ```csharp
   [HttpGet("orders")]
   [Authorize]  // ? Xem danh sách order c?a mình
   public async Task<IActionResult> GetMyOrders()
   {
       // Protected endpoint
   }
   ```

3. **Endpoint th?c hi?n actions quan tr?ng**
   ```csharp
   [HttpPost("purchase")]
   [Authorize]  // ? Ph?i ??ng nh?p ?? mua hàng
   public async Task<IActionResult> MakePurchase()
   {
       // Critical operation
   }
   ```

### ? **KHÔNG c?n [Authorize]** khi:

1. **Public endpoints - Ai c?ng có th? access**
   ```csharp
   [HttpPost("login")]
   // Không có [Authorize] - Ai c?ng có th? login
   public async Task<IActionResult> Login([FromBody] LoginRequest request)
   {
       // Public endpoint
   }
   
   [HttpPost("register")]
   // Không có [Authorize] - Ai c?ng có th? ??ng ký
   public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
   {
       // Public endpoint
   }
   ```

2. **Public data - Thông tin công khai**
   ```csharp
   [HttpGet("products")]
   // Không c?n [Authorize] - Xem s?n ph?m là public
   public async Task<IActionResult> GetProducts()
   {
       // Anyone can view products
   }
   
   [HttpGet("products/{id}")]
   // Public product details
   public async Task<IActionResult> GetProduct(Guid id)
   {
       // Anyone can view product details
   }
   ```

3. **Health check, Status endpoints**
   ```csharp
   [HttpGet("health")]
   // Không c?n authorize cho health check
   public IActionResult HealthCheck()
   {
       return Ok("Healthy");
   }
   ```

## ?? Các cách s? d?ng [Authorize]

### 1. **Basic [Authorize] - Ch? c?n authenticated**
```csharp
[Authorize]
[HttpGet("me")]
public async Task<IActionResult> GetMe()
{
    // Ch? c?n user có valid token
    // B?t k? authenticated user nào c?ng access ???c
}
```

**Yêu c?u:**
- ? User ph?i có valid JWT token
- ? Token ch?a expire
- ? Token signature h?p l?

### 2. **[Authorize(Roles = "...")] - Yêu c?u specific role**
```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("users/{id}")]
public async Task<IActionResult> DeleteUser(Guid id)
{
    // Ch? Admin m?i xóa ???c user
}
```

**Yêu c?u:**
- ? User ph?i có valid token
- ? User ph?i có role "Admin"

### 3. **[Authorize(Roles = "Role1,Role2")] - Multiple roles (OR)**
```csharp
[Authorize(Roles = "Admin,Manager")]
[HttpGet("reports")]
public async Task<IActionResult> GetReports()
{
    // User ph?i có role Admin HO?C Manager
}
```

**Yêu c?u:**
- ? User có role "Admin" **HO?C**
- ? User có role "Manager"

### 4. **Multiple [Authorize] - Multiple roles (AND)**
```csharp
[Authorize(Roles = "Admin")]
[Authorize(Roles = "Manager")]
[HttpPost("critical-action")]
public async Task<IActionResult> CriticalAction()
{
    // User ph?i có C? Admin VÀ Manager
}
```

**Yêu c?u:**
- ? User có role "Admin" **VÀ**
- ? User có role "Manager"

### 5. **[Authorize(Policy = "...")] - Custom policy**
```csharp
[Authorize(Policy = "RequireAdminAndDeleteAuthority")]
[HttpDelete("products/{id}")]
public async Task<IActionResult> DeleteProduct(Guid id)
{
    // Áp d?ng custom policy ?ã ??nh ngh?a
}
```

**C?u hình Policy trong Program.cs:**
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminAndDeleteAuthority", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("authority", "DELETE");
    });
});
```

## ?? So sánh các cách s? d?ng

| Attribute | Yêu c?u | Use Case |
|-----------|---------|----------|
| `[Authorize]` | Valid token | Basic authentication |
| `[Authorize(Roles = "Admin")]` | Admin role | Admin-only features |
| `[Authorize(Roles = "Admin,Manager")]` | Admin OR Manager | Multiple roles allowed |
| Multiple `[Authorize]` | Admin AND Manager | Strict role requirements |
| `[Authorize(Policy = "...")]` | Custom logic | Complex authorization |

## ?? Examples trong MyShop

### AuthController.cs
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    // ? No [Authorize] - Public endpoint
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        // Anyone can register
        return Ok(await _authService.RegisterAsync(request));
    }

    // ? No [Authorize] - Public endpoint
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Anyone can login
        return Ok(await _authService.LoginAsync(request));
    }

    // ? [Authorize] - Protected endpoint
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        // Only authenticated users
        return Ok(await _authService.GetMeAsync());
    }
}
```

### UserController.cs
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // ? T?t c? endpoints trong controller này c?n authentication
public class UserController : ControllerBase
{
    // Inherited [Authorize] from controller
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        // Protected by controller-level [Authorize]
    }

    // Có th? override v?i [AllowAnonymous]
    [HttpGet("public-profile/{id}")]
    [AllowAnonymous] // ? Override controller [Authorize]
    public async Task<IActionResult> GetPublicProfile(Guid id)
    {
        // Public endpoint despite controller [Authorize]
    }

    // Thêm role requirement
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // ? Override + Add role requirement
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        // Only Admin can delete
    }
}
```

### ProductController.cs
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class ProductController : ControllerBase
{
    // ? No [Authorize] - Public viewing
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        // Anyone can view products
    }

    // ? No [Authorize] - Public viewing
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        // Anyone can view product details
    }

    // ? [Authorize] - Need to be logged in
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        // Must be authenticated to create
    }

    // ? [Authorize] v?i Role
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        // Only Admin or Manager can update
    }

    // ? [Authorize] v?i Role strict
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        // Only Admin can delete
    }
}
```

## ?? Custom Authorization Policies

### ??nh ngh?a trong Program.cs
```csharp
builder.Services.AddAuthorization(options =>
{
    // Policy 1: Require Admin role + ALL authority
    options.AddPolicy("SuperAdmin", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("authority", "ALL");
    });

    // Policy 2: Require any management role
    options.AddPolicy("Management", policy =>
    {
        policy.RequireRole("Admin", "Manager", "Supervisor");
    });

    // Policy 3: Require DELETE authority
    options.AddPolicy("CanDelete", policy =>
    {
        policy.RequireClaim("authority", "DELETE");
    });

    // Policy 4: Complex logic - Admin OR (Manager WITH POST authority)
    options.AddPolicy("CanCreateContent", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var hasAdminRole = context.User.IsInRole("Admin");
            var hasManagerRole = context.User.IsInRole("Manager");
            var hasPostAuthority = context.User.HasClaim("authority", "POST");
            
            return hasAdminRole || (hasManagerRole && hasPostAuthority);
        });
    });

    // Policy 5: Email verified requirement
    options.AddPolicy("EmailVerified", policy =>
    {
        policy.RequireAssertion(context =>
        {
            // Assuming we add IsVerified to token claims
            var isVerified = context.User.FindFirst("email_verified")?.Value;
            return isVerified == "true";
        });
    });
});
```

### S? d?ng Custom Policies
```csharp
[ApiController]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    [HttpPost("critical-action")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> CriticalAction()
    {
        // Only Super Admins (Admin + ALL authority)
    }

    [HttpGet("reports")]
    [Authorize(Policy = "Management")]
    public async Task<IActionResult> GetReports()
    {
        // Any management role can access
    }

    [HttpDelete("content/{id}")]
    [Authorize(Policy = "CanDelete")]
    public async Task<IActionResult> DeleteContent(Guid id)
    {
        // Anyone with DELETE authority
    }

    [HttpPost("articles")]
    [Authorize(Policy = "CanCreateContent")]
    public async Task<IActionResult> CreateArticle()
    {
        // Complex authorization logic
    }
}
```

## ?? Best Practices

### 1. **Default Secure - Explicit Open**
```csharp
// ? GOOD - Controller level protection, explicit public endpoints
[ApiController]
[Authorize] // ? Default: All endpoints need auth
public class UserController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // Protected
    }

    [HttpGet("public")]
    [AllowAnonymous] // ? Explicit: This is public
    public async Task<IActionResult> GetPublicInfo()
    {
        // Public
    }
}

// ? BAD - Easy to forget [Authorize] on sensitive endpoints
[ApiController]
public class UserController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // Oops! Forgot [Authorize] - Now it's public!
    }

    [HttpGet("public")]
    public async Task<IActionResult> GetPublicInfo()
    {
        // Should be public but no indication
    }
}
```

### 2. **Use Policies for Complex Logic**
```csharp
// ? GOOD - Policy encapsulates logic
[Authorize(Policy = "CanManageUsers")]
public async Task<IActionResult> ManageUser()
{
    // Clean and reusable
}

// ? BAD - Logic in controller
[Authorize]
public async Task<IActionResult> ManageUser()
{
    if (!User.IsInRole("Admin") && 
        !User.HasClaim("authority", "MANAGE_USERS"))
    {
        return Forbid();
    }
    // Duplicated logic everywhere
}
```

### 3. **Document Authorization Requirements**
```csharp
/// <summary>
/// Delete a product
/// </summary>
/// <remarks>
/// **Authorization:** Requires Admin role + DELETE authority
/// </remarks>
[HttpDelete("{id}")]
[Authorize(Policy = "CanDelete")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteProduct(Guid id)
{
    // Clear documentation
}
```

## ?? Quick Decision Tree

```
Is this endpoint public?
?? YES ? No [Authorize] needed
?         Examples: Login, Register, View Products, Health Check
?
?? NO ? Does it need specific roles?
        ?? YES ? Does it need ONE of multiple roles?
        ?        ?? YES ? [Authorize(Roles = "Role1,Role2")]
        ?        ?? NO (needs ALL roles) ? Multiple [Authorize(Roles = "...")]
        ?
        ?? NO ? Does it need complex authorization?
                ?? YES ? [Authorize(Policy = "CustomPolicy")]
                ?? NO ? [Authorize] (just need authentication)
```

## ?? Testing Authorization

### Test v?i HTTP file
```http
### Public endpoint - No token needed
GET http://localhost:5228/api/v1/products

###

### Protected endpoint - Token required
GET http://localhost:5228/api/v1/auth/me
Authorization: Bearer {{token}}

###

### Admin only - Admin token required
DELETE http://localhost:5228/api/v1/users/{{userId}}
Authorization: Bearer {{adminToken}}

###

### Test unauthorized (should return 401)
GET http://localhost:5228/api/v1/auth/me

###

### Test forbidden (should return 403)
DELETE http://localhost:5228/api/v1/users/{{userId}}
Authorization: Bearer {{userToken}}
# User token (not admin) - Should get 403 Forbidden
```

## ?? Summary

| Scenario | Solution | Example |
|----------|----------|---------|
| Public access | No `[Authorize]` | Login, Register, View Products |
| Need authentication | `[Authorize]` | Get Profile, View Orders |
| Admin only | `[Authorize(Roles = "Admin")]` | Delete Users, System Config |
| Admin OR Manager | `[Authorize(Roles = "Admin,Manager")]` | View Reports |
| Admin AND Manager | Multiple `[Authorize]` | Critical Actions |
| Complex logic | `[Authorize(Policy = "...")]` | Custom Requirements |

**Remember:**
- ?? Default to secure ([Authorize])
- ?? Use [AllowAnonymous] for explicit public endpoints
- ?? Use Policies for reusable complex authorization
- ?? Document authorization requirements
- ?? Test both authorized and unauthorized scenarios