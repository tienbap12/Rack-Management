using Microsoft.AspNetCore.Http;
using Rack.Application.Commons.Interfaces;
using System.Linq;
using System.Security.Claims;

namespace Rack.MainInfrastructure.Common.Authentication
{
    public class UserContext(IHttpContextAccessor _httpContextAccessor) : IUserContext
    {
        public string GetRole()
        {
            return _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        }

        public string GetUsername()
        {
            return _httpContextAccessor.HttpContext?.User?.Claims?
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        }
    }
}