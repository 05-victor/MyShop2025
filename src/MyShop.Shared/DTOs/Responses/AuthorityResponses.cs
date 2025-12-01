using System;

namespace MyShop.Shared.DTOs.Responses
{
    /// <summary>
    /// Response DTO for removed authority information of a user.
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
    /// Response DTO for the list of effective authorities of a user.
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
    /// Response DTO for authority check result.
    /// </summary>
    public class CheckAuthorityResponse
    {
        public Guid UserId { get; set; }
        public string AuthorityName { get; set; } = string.Empty;
        public bool HasAuthority { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
