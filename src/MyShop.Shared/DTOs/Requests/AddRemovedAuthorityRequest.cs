using System;
using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests
{
    /// <summary>
    /// Request DTO to add an authority to user's removed authorities list.
    /// </summary>
    public class AddRemovedAuthorityRequest
    {
        /// <summary>
        /// Name of the authority to remove.
        /// </summary>
        [Required(ErrorMessage = "Authority name is required")]
        [MaxLength(100, ErrorMessage = "Authority name cannot exceed 100 characters")]
        public string AuthorityName { get; set; } = string.Empty;

        /// <summary>
        /// Reason for removing the authority (optional).
        /// </summary>
        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }

        /// <summary>
        /// Username of the person performing the removal (optional).
        /// </summary>
        [MaxLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
        public string? RemovedBy { get; set; }
    }
}
