# IHttpContextAccessor và HttpContext - Chi ti?t ho?t ??ng

## ?? T?ng quan

`IHttpContextAccessor` là m?t interface trong ASP.NET Core cho phép **truy c?p `HttpContext` t? b?t k? ?âu** trong ?ng d?ng, không ch? trong Controller.

## ?? ??nh ngh?a

### IHttpContextAccessor Interface
```csharp
namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// Provides access to the current HttpContext, if one is available.
    /// </summary>
    public interface IHttpContextAccessor
    {
        /// <summary>
        /// Gets or sets the current HttpContext.
        /// Returns null if there is no active HTTP request.
        /// </summary>
        HttpContext? HttpContext { get; set; }
    }
}
```

### HttpContext Class
```csharp
namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// Encapsulates all HTTP-specific information about an individual HTTP request.
    /// </summary>
    public abstract class HttpContext
    {
        // Request information
        public abstract HttpRequest Request { get; }
        
        // Response information
        public abstract HttpResponse Response { get; }
        
        // Connection information
        public abstract ConnectionInfo Connection { get; }
        
        // User authentication/authorization
        public abstract ClaimsPrincipal User { get; set; }
        
        // Session data
        public abstract ISession Session { get; set; }
        
        // Request services
        public abstract IServiceProvider RequestServices { get; set; }
        
        // And many more properties...
    }
}
```

## ?? Cách ho?t ??ng

### 1. **??ng ký Service** (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

// ? ??ng ký IHttpContextAccessor vào DI container
builder.Services.AddHttpContextAccessor();
```

**?i?u này làm gì?**
- ??ng ký `HttpContextAccessor` implementation c?a `IHttpContextAccessor`
- ASP.NET Core t? ??ng inject `HttpContext` hi?n t?i vào accessor này
- Lifecycle: **Singleton** - ch? có 1 instance trong su?t lifetime c?a app

### 2. **Inject vào Service** (AuthService.cs)

```csharp
public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Constructor injection
    public AuthService(
        IUserRepository userRepository, 
        IJwtService jwtService, 
        IRoleRepository roleRepository, 
        ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor)  // ? Inject ? ?ây
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _roleRepository = roleRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;  // ? L?u vào field
    }
}
```

### 3. **S? d?ng trong Method**

```csharp
public async Task<UserInfoResponse?> GetMeAsync()
{
    // L?y HttpContext hi?n t?i
    var httpContext = _httpContextAccessor.HttpContext;
    
    if (httpContext == null)
    {
        // Không có HTTP request ?ang active
        // (VD: g?i t? background job, console app, etc.)
        return null;
    }

    // Truy c?p User claims t? JWT token
    var userId = InfoJwtService.GetUserId(httpContext.User);
    
    // Có th? truy c?p nhi?u th? khác:
    // - httpContext.Request - HTTP request details
    // - httpContext.Response - HTTP response
    // - httpContext.User - Authenticated user claims
    // - httpContext.Connection - Connection info
    // - httpContext.Session - Session data
}
```

## ?? Request Flow (Lu?ng ho?t ??ng)

```
1. HTTP Request ??n Server
   ?
2. ASP.NET Core t?o HttpContext cho request này
   ?
3. HttpContext ???c set vào HttpContextAccessor.HttpContext
   ?
4. Authentication Middleware x? lý JWT token
   ?
5. Claims ???c add vào HttpContext.User
   ?
6. Request ??n Controller
   ?
7. Controller g?i AuthService.GetMeAsync()
   ?
8. AuthService.GetMeAsync() truy c?p _httpContextAccessor.HttpContext
   ?
9. L?y ???c HttpContext.User ch?a claims t? JWT token
   ?
10. Extract userId t? claims
   ?
11. Query database và return user info
   ?
12. Response tr? v? client
```

## ?? HttpContext ch?a nh?ng gì?

### HttpContext.Request
```csharp
var httpContext = _httpContextAccessor.HttpContext;

// URL information
var url = httpContext.Request.Path;           // "/api/v1/auth/me"
var method = httpContext.Request.Method;      // "GET"
var queryString = httpContext.Request.Query;  // Query parameters

// Headers
var authHeader = httpContext.Request.Headers["Authorization"];
var contentType = httpContext.Request.ContentType;

// Body (for POST/PUT)
var body = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
```

### HttpContext.Response
```csharp
// Status code
httpContext.Response.StatusCode = 200;

// Headers
httpContext.Response.Headers.Add("X-Custom-Header", "value");

// Body
await httpContext.Response.WriteAsync("Response content");
```

### HttpContext.User (JWT Claims) ?
```csharp
// Authenticated user information
var user = httpContext.User;

// Check if authenticated
if (user.Identity?.IsAuthenticated == true)
{
    // Get claims
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var username = user.FindFirst(ClaimTypes.Name)?.Value;
    var email = user.FindFirst(ClaimTypes.Email)?.Value;
    var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
}
```

### HttpContext.Connection
```csharp
// Client IP address
var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();

// Connection ID
var connectionId = httpContext.Connection.Id;
```

## ?? Lifetime và Thread Safety

### Lifetime
- `IHttpContextAccessor`: **Singleton** - 1 instance cho c? app
- `HttpContext`: **Per Request** - m?i HTTP request có 1 instance riêng

### Thread Safety
```csharp
// ? SAFE - Trong request scope
public async Task<UserInfoResponse?> GetMeAsync()
{
    var httpContext = _httpContextAccessor.HttpContext;
    // HttpContext này ch? thu?c v? request hi?n t?i
}

// ?? CAUTION - Background tasks
Task.Run(() => 
{
    var httpContext = _httpContextAccessor.HttpContext;
    // httpContext có th? null ho?c thu?c request khác!
});
```

## ?? Use Cases

### 1. **Extract User t? JWT Token** (Use case c?a b?n)
```csharp
public async Task<UserInfoResponse?> GetMeAsync()
{
    var httpContext = _httpContextAccessor.HttpContext;
    var userId = InfoJwtService.GetUserId(httpContext.User);
    
    return await GetUserByIdAsync(userId.Value);
}
```

### 2. **Logging v?i Client Info**
```csharp
public void LogActivity(string action)
{
    var httpContext = _httpContextAccessor.HttpContext;
    var userId = InfoJwtService.GetUserId(httpContext.User);
    var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
    
    _logger.LogInformation(
        "User {UserId} performed {Action} from IP {ClientIp}",
        userId, action, clientIp);
}
```

### 3. **Custom Authorization Logic**
```csharp
public bool CanAccessResource(string resourceId)
{
    var httpContext = _httpContextAccessor.HttpContext;
    var authorities = InfoJwtService.GetAuthorities(httpContext.User);
    
    return authorities.Contains("ALL") || 
           authorities.Contains("READ");
}
```

### 4. **Audit Trail**
```csharp
public async Task AuditAction(string entityType, string action)
{
    var httpContext = _httpContextAccessor.HttpContext;
    
    var audit = new AuditLog
    {
        UserId = InfoJwtService.GetUserId(httpContext.User),
        Username = InfoJwtService.GetUsername(httpContext.User),
        Action = action,
        EntityType = entityType,
        IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
        UserAgent = httpContext.Request.Headers["User-Agent"],
        Timestamp = DateTime.UtcNow
    };
    
    await _auditRepository.SaveAsync(audit);
}
```

## ?? L?u ý quan tr?ng

### 1. **Null Check Required**
```csharp
var httpContext = _httpContextAccessor.HttpContext;
if (httpContext == null)
{
    // Không có HTTP request active
    // Có th? x?y ra khi:
    // - G?i t? background job
    // - G?i t? console app
    // - G?i t? unit test
    return null;
}
```

### 2. **Only Works in Request Scope**
```csharp
// ? BAD - HttpContext có th? null ho?c wrong request
private HttpContext? _cachedContext;

public void CacheContext()
{
    _cachedContext = _httpContextAccessor.HttpContext;
}

public void UseContextLater()
{
    // _cachedContext có th? ?ã thu?c v? request khác!
    var user = _cachedContext?.User;
}

// ? GOOD - Always get fresh HttpContext
public void DoSomething()
{
    var httpContext = _httpContextAccessor.HttpContext;
    if (httpContext != null)
    {
        var user = httpContext.User;
    }
}
```

### 3. **Performance Consideration**
```csharp
// ? AVOID - Multiple access
public void Process()
{
    var user1 = _httpContextAccessor.HttpContext?.User;
    // ... code ...
    var user2 = _httpContextAccessor.HttpContext?.User;
    // ... code ...
    var user3 = _httpContextAccessor.HttpContext?.User;
}

// ? BETTER - Cache locally
public void Process()
{
    var httpContext = _httpContextAccessor.HttpContext;
    if (httpContext == null) return;
    
    var user = httpContext.User;
    // Use 'user' multiple times
}
```

## ?? Testing

### Mock IHttpContextAccessor trong Unit Tests
```csharp
[Fact]
public async Task GetMeAsync_WithValidToken_ReturnsUserInfo()
{
    // Arrange
    var userId = Guid.NewGuid();
    
    // Mock HttpContext
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, userId.ToString())
    };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);
    
    var httpContextMock = new Mock<HttpContext>();
    httpContextMock.Setup(x => x.User).Returns(claimsPrincipal);
    
    // Mock HttpContextAccessor
    var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);
    
    // Create service with mocked accessor
    var authService = new AuthService(
        userRepository,
        jwtService,
        roleRepository,
        logger,
        httpContextAccessorMock.Object);
    
    // Act
    var result = await authService.GetMeAsync();
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(userId, result.Id);
}
```

## ?? So sánh v?i Controller

### Trong Controller (Không c?n IHttpContextAccessor)
```csharp
[ApiController]
public class AuthController : ControllerBase
{
    // HttpContext có s?n qua base.HttpContext
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // User là shortcut cho HttpContext.User
        
        var clientIp = HttpContext.Connection.RemoteIpAddress;
        // HttpContext có s?n trong Controller
    }
}
```

### Trong Service (C?n IHttpContextAccessor)
```csharp
public class AuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public async Task<UserInfoResponse?> GetMeAsync()
    {
        // Ph?i dùng accessor ?? l?y HttpContext
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
```

## ?? Best Practices

1. ? **Always null check** tr??c khi dùng HttpContext
2. ? **Cache locally** n?u dùng nhi?u l?n trong 1 method
3. ? **Inject IHttpContextAccessor**, không dùng static
4. ? **Extract claims s?m** và pass as parameters
5. ? **Không cache HttpContext** across requests
6. ? **Không dùng trong background jobs** mà không check null
7. ? **Không pass HttpContext** qua async boundaries

## ?? References

- [IHttpContextAccessor Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.ihttpcontextaccessor)
- [HttpContext Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext)
- [ASP.NET Core Request Features](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/request-features)