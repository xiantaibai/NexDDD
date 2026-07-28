using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.Auther;

namespace NexDDD.Core.MultiTenants.Services
{
    /// <summary>
    /// 负责多租户模块中的 DefaultClaimsPrincipalStrategy 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
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
