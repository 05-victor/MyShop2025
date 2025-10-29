using MyShop.Client.Core.Services.Interfaces;
using MyShop.Client.Core.Strategies;
using MyShop.Client.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShop.Client.Core.Services.Implementations
{
    /// <summary>
    /// Factory implementation cho role strategies
    /// </summary>
    public class RoleStrategyFactory : IRoleStrategyFactory
    {
        private readonly Dictionary<UserRole, IRoleStrategy> _strategies;

        public RoleStrategyFactory(IEnumerable<IRoleStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(s => s.Role);
        }

        public IRoleStrategy GetStrategy(UserRole role)
        {
            if (_strategies.TryGetValue(role, out var strategy))
            {
                return strategy;
            }

            throw new ArgumentException($"No strategy found for role: {role}");
        }
    }
}
