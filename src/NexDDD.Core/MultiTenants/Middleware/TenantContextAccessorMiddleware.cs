using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.MultiTenants.Middleware
{
    /// <summary>
    /// 提供多租户模块的运行上下文，协调状态生命周期及其与依赖组件的交互。
    /// </summary>
    public class TenantContextAccessorMiddleware<T>(
        RequestDelegate next,
        IHttpContextAccessor httpContextAccessor,
        ITenantContextAccessor<T> TenantAccessor,
        ITenantStore<T> TenantResolver,
        ITenantResolver TenantResolutionStrategy,
        IOptions<TenantSettings<T>> options) where T : Tenant
    {

        /// <summary>
        /// 设置 租户上下文
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            var setting = options.Value!;

            // 上下文为空 设置租户上下文
            httpContextAccessor.HttpContext ??= context;

            var identifier = TenantResolutionStrategy.GetTenantId();
            if (identifier == null)
                throw new InvalidOperationException("Tenant identifier could not be resolved using configured strategy");

            var tenant = await TenantResolver.TryGetAsync(identifier);
            if (tenant == null && identifier != 0)
                throw new InvalidOperationException($"No tenant found matching '{identifier}'");

            TenantAccessor.TenantInfo ??= tenant;

            await next.Invoke(context);
        }
    }
}
