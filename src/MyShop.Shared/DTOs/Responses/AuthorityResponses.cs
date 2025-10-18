using System;

namespace MyShop.Shared.DTOs.Responses
{
    /// <summary>
    /// Response DTO cho thông tin quyền hạn bị loại bỏ của user.
    /// </summary>
    public class RemovedAuthorityResponse
    {
        public Guid UserId { get; set; }
        public string AuthorityName { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime RemovedAt { get; set; }
        public string? RemovedBy { get; set; }
    }

    /// <summary>
    /// Response DTO cho danh sách quyền hạn hiệu lực của user.
    /// </summary>
    public class EffectiveAuthoritiesResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<string> RoleNames { get; set; } = new List<string>();
        public List<string> AllAuthoritiesFromRoles { get; set; } = new List<string>();
        public List<string> RemovedAuthorities { get; set; } = new List<string>();
        public List<string> EffectiveAuthorities { get; set; } = new List<string>();
    }

    /// <summary>
    /// Response DTO cho kết quả kiểm tra quyền hạn.
    /// </summary>
    public class CheckAuthorityResponse
    {
        public Guid UserId { get; set; }
        public string AuthorityName { get; set; } = string.Empty;
        public bool HasAuthority { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
