using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.MultiTenants.Services;

namespace NexDDD.Core.MultiTenants
{

    /// <summary>
    /// 负责多租户模块中的 TenantBuilder 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public class TenantBuilder<T> where T : Tenant
    {
        private readonly IServiceCollection _services;

        public TenantBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public TenantBuilder<T> WithClaimsPrincipalStrategy<V>() where V : class, IClaimsPrincipalStrategy
        {
            _services.TryAddSingleton(typeof(IClaimsPrincipalStrategy), typeof(V));
            return this;
        }

        /// <summary>
        /// 注册 解析方式
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public TenantBuilder<T> WithResolver<V>() where V : class, ITenantResolver
        {
            _services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            _services.TryAddSingleton(typeof(ITenantResolver), typeof(V));
            return this;
        }

        /// <summary>
        /// 注册 默认实现
        /// </summary>
        /// <returns></returns>
        public TenantBuilder<T> WithDefaultResolver()
        {
            WithClaimsPrincipalStrategy<DefaultClaimsPrincipalStrategy>();
            return WithResolver<HttpContextTenantResolver>();
        }



        /// <summary>
        /// 注册 存储方式,必须为单例
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public TenantBuilder<T> WithStore<V>() where V : class, ITenantStore<T>
        {
            _services.TryAddSingleton<ITenantStore<T>, V>();
            return this;
        }

        /// <summary>
        /// 注册默认的存储
        /// </summary>
        /// <returns></returns>
        public TenantBuilder<T> WithDefaultStore()
        {
            return WithStore<DefaultTenantStore<T>>();
        }


    }
}
