using System.Security.Claims;

namespace MyShop.Server.Services.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

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
}
