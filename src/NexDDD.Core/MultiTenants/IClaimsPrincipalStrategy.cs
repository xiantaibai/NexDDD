using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.MultiTenants
{
    /// <summary>
    /// 定义多租户模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface IClaimsPrincipalStrategy
    {
        string? GetUserId(ClaimsPrincipal principal);
        string? GetUserName(ClaimsPrincipal principal);
        string? GetTenantId(ClaimsPrincipal principal);

        string? GetUserRole(ClaimsPrincipal principal);
    }
}
