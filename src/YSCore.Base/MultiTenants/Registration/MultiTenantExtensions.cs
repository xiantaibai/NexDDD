using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.MultiTenants.Middleware;

namespace YSCore.Base.MultiTenants
{
    public static class MultiTenantExtensions
    {
        /// <summary>
        /// 自定义扩展 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static TenantBuilder<T> AddMultiTenancy<T>(this IServiceCollection services, Action<TenantSettings<T>>? configure = null) where T : Tenant
        {
            var options = new TenantSettings<T>();
            configure?.Invoke(options);
            services.Configure<TenantSettings<T>>(o => configure?.Invoke(o));

            services.TryAddSingleton<ITenantContextAccessor<T>, AsyncLocalTenantContextAccessor<T>>();

            return new TenantBuilder<T>(services);
        }

        /// <summary>
        /// 注册默认的 Tenant
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static TenantBuilder<Tenant> AddMultiTenancy(this IServiceCollection services, Action<TenantSettings<Tenant>>? configure = null)
        {
            return AddMultiTenancy<Tenant>(services, configure);
        }


        /// <summary>
        /// Use the Teanant Middleware to process the request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMultiTenancy<T>(this IApplicationBuilder builder) where T : Tenant
            => builder.UseMiddleware<TenantContextAccessorMiddleware<T>>();

        /// <summary>
        /// 默认
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder builder)
       => UseMultiTenancy<Tenant>(builder);
    }
}
