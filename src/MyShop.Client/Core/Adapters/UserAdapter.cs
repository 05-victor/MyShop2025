using MyShop.Client.Models;
using MyShop.Client.Models.Enums;
using MyShop.Shared.DTOs.Responses;
using System;
using System.Linq;

namespace MyShop.Client.Core.Adapters
{
    /// <summary>
    /// Adapter pattern: Transform DTOs thành Domain Models
    /// </summary>
    public static class UserAdapter
    {
        /// <summary>
        /// Convert LoginResponse DTO → User domain model
        /// </summary>
        public static User FromLoginResponse(LoginResponse dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new User
            {
                Id = dto.Id,
                Username = dto.Username,
                Email = dto.Email,
                CreatedAt = dto.CreatedAt,
                IsTrialActive = dto.IsTrialActive,
                TrialStartDate = dto.TrialStartDate,
                TrialEndDate = dto.TrialEndDate,
                IsEmailVerified = dto.IsEmailVerified,
                Token = dto.Token,
                Roles = ParseRoles(dto.RoleNames)
            };
        }

        /// <summary>
        /// Convert UserInfoResponse DTO → User domain model
        /// </summary>
        public static User FromUserInfoResponse(UserInfoResponse dto, string token = "")
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new User
            {
                Id = dto.Id,
                Username = dto.Username,
                Email = dto.Email,
                CreatedAt = dto.CreatedAt,
                Token = token,
                Roles = ParseRoles(dto.RoleNames)
            };
        }

        /// <summary>
        /// Parse role names (string) → UserRole enums
        /// </summary>
        private static System.Collections.Generic.List<UserRole> ParseRoles(System.Collections.Generic.IEnumerable<string>? roleNames)
        {
            if (roleNames == null)
                return new System.Collections.Generic.List<UserRole>();

            var roles = new System.Collections.Generic.List<UserRole>();

            foreach (var roleName in roleNames)
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    continue;

                var normalized = roleName.Trim().ToUpperInvariant();

                if (normalized == "ADMIN")
                    roles.Add(UserRole.Admin);
                else if (normalized == "SALEMAN" || normalized == "SALESMAN")
                    roles.Add(UserRole.Salesman);
                else if (normalized == "CUSTOMER")
                    roles.Add(UserRole.Customer);
            }

            // Default to Customer if no roles found
            if (roles.Count == 0)
                roles.Add(UserRole.Customer);

            return roles;
        }
    }
}
