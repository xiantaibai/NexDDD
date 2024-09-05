using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.MultiTenants
{
    /// <summary>
    /// 从上下文 获取 租户Id
    /// </summary>
    public class HttpContextTenantResolver(IHttpContextAccessor httpContextAccessor) : ITenantResolver
    {
        public long GetTenantId()
        {
            if (httpContextAccessor.HttpContext == null)
                throw new InvalidOperationException("HttpContext is not available");

            if (httpContextAccessor.HttpContext.Request.Headers.TryGetValue("x-tenant", out var tenantId))
            {
                if (long.TryParse(tenantId, out long result))
                {
                    return result;
                }
            }
            return 0;
        }
    }
}
