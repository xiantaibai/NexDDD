using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.Auther;

namespace YSCore.Base.MultiTenants.Services
{
    public class DefaultClaimsPrincipalStrategy : IClaimsPrincipalStrategy
    {
        public string? GetTenantId(ClaimsPrincipal principal)
        {
            return principal.FindFirst(AppClaimTypes.TenantId)?.Value;
        }

        public string? GetUserId(ClaimsPrincipal principal)
        {
            return principal.FindFirst(AppClaimTypes.UserId)?.Value;
        }

        public string? GetUserName(ClaimsPrincipal principal)
        {
            return principal.FindFirst(AppClaimTypes.UserName)?.Value;
        }

        public string? GetUserRole(ClaimsPrincipal principal)
        {
            return principal.FindFirst(AppClaimTypes.UserRole)?.Value;
        }
    }

}
