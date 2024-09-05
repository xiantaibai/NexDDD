using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.MultiTenants
{

    public class TenantBuilder<T> where T : Tenant
    {
        private readonly IServiceCollection _services;

        public TenantBuilder(IServiceCollection services)
        {
            _services = services;
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
        /// 注册HttpContext
        /// </summary>
        /// <returns></returns>
        public TenantBuilder<T> WithHttpContextResolver()
        {
            return WithResolver<HttpContextTenantResolver>();
        }

        /// <summary>
        /// 注册 存储方式
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public TenantBuilder<T> WithStore<V>(ServiceLifetime lifetime = ServiceLifetime.Transient) where V : class, ITenantStore<T>
        {
            _services.Add(ServiceDescriptor.Describe(typeof(ITenantStore<T>), typeof(V), lifetime));
            // _services.TryAddScoped<ITenantStore<T>, V>();
            return this;
        }

        /// <summary>
        /// 注册默认的存储
        /// </summary>
        /// <returns></returns>
        public TenantBuilder<T> WithDefaultStore(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            //var service = new DefaultTenantStore<T>(tenants);
            //_services.TryAddScoped<ITenantStore<T>>(service);
            return WithStore<DefaultTenantStore<T>>();
        }


    }
}
