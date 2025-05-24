using Microsoft.AspNetCore.Http;
using Rack.Domain.Commons.Abstractions;
using System;
using System.Linq;
using System.Security.Claims;

namespace Rack.MainInfrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return userId != null ? Guid.Parse(userId) : Guid.Empty;
            }
        }

        public string Username
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            }
        }

        public string Role
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            }
        }

        public string[] Roles
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?
                    .FindAll(ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToArray() ?? Array.Empty<string>();
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
        }

        public bool IsInRole(string role)
        {
            // First try the single role (for backward compatibility)
            if (!string.IsNullOrEmpty(Role) &&
                string.Equals(Role, role, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check in the roles array
            return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }
    }
}