using System.Security.Claims;

namespace MyShop.Server.Services.Implementations
{
    /// <summary>
    /// Service đơn giản để lấy thông tin từ JWT token
    /// </summary>
    public class InfoJwtService
    {
        /// <summary>
        /// Lấy User ID từ ClaimsPrincipal
        /// Thử nhiều claim types để tương thích với cả MapInboundClaims true/false
        /// </summary>
        public static Guid? GetUserId(ClaimsPrincipal user)
        {
            // Try standard ClaimTypes.NameIdentifier first
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // If not found, try "sub" (JWT standard claim for subject/user ID)
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = user.FindFirst("sub")?.Value;
            }
            
            // If still not found, try "nameid" (used when MapInboundClaims = false)
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = user.FindFirst("nameid")?.Value;
            }
            
            // Try to parse the claim value to Guid
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        /// <summary>
        /// Lấy Username từ ClaimsPrincipal
        /// </summary>
        public static string? GetUsername(ClaimsPrincipal user)
        {
            // Try ClaimTypes.Name first
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            
            // If not found, try "unique_name" (used when MapInboundClaims = false)
            if (string.IsNullOrEmpty(username))
            {
                username = user.FindFirst("unique_name")?.Value;
            }
            
            return username;
        }

        /// <summary>
        /// Lấy Email từ ClaimsPrincipal
        /// </summary>
        public static string? GetEmail(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Lấy danh sách Roles từ ClaimsPrincipal
        /// </summary>
        public static List<string> GetRoles(ClaimsPrincipal user)
        {
            return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }

        /// <summary>
        /// Lấy danh sách Authorities từ ClaimsPrincipal
        /// </summary>
        public static List<string> GetAuthorities(ClaimsPrincipal user)
        {
            return user.FindAll("authority").Select(c => c.Value).ToList();
        }

        /// <summary>
        /// Kiểm tra user có authority cụ thể hay không
        /// </summary>
        public static bool HasAuthority(ClaimsPrincipal user, string authority)
        {
            return user.HasClaim("authority", authority);
        }

        /// <summary>
        /// Lấy JWT ID (JTI)
        /// </summary>
        public static string? GetJti(ClaimsPrincipal user)
        {
            return user.FindFirst("jti")?.Value;
        }
    }
}