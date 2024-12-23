using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.MultiTenants
{
    public interface IClaimsPrincipalStrategy
    {
        string? GetUserId(ClaimsPrincipal principal);
        string? GetUserName(ClaimsPrincipal principal);
        string? GetTenantId(ClaimsPrincipal principal);

        string? GetUserRole(ClaimsPrincipal principal);
    }
}
