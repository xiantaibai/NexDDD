using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.Auther;

namespace NexDDD.Core.MultiTenants
{
    /// <summary>
    /// 从上下文 获取 租户Id
    /// 负责多租户模块中的 HttpContextTenantResolver 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    public class HttpContextTenantResolver(IHttpContextAccessor httpContextAccessor, IClaimsPrincipalStrategy claimsPrincipalStrategy) : ITenantResolver
    {
        public long GetTenantId()
        {
            if (httpContextAccessor.HttpContext == null)
                throw new InvalidOperationException("HttpContext is not available");

            if (httpContextAccessor.HttpContext.Request.Headers.TryGetValue(AppClaimTypes.TenantId, out var tenantId))
            {
                if (long.TryParse(tenantId, out long result))
                {
                    return result;
                }
            }
            tenantId = claimsPrincipalStrategy.GetTenantId(httpContextAccessor.HttpContext.User);
            if (!string.IsNullOrEmpty(tenantId))
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
