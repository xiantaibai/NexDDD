using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Authorization
{
    public static class AuthorizationServiceCollectionExtensions
    {
        public static IServiceCollection AddAppAuthorization<TAuthorizationHandler>(this IServiceCollection services, Action<IServiceCollection> configure = null)
             where TAuthorizationHandler : class, IAuthorizationHandler
        {
            // 注册授权策略提供器
            services.TryAddSingleton<IAuthorizationPolicyProvider, AppAuthorizationPolicyProvider>();

            // 注册策略授权处理程序
            services.TryAddSingleton<IAuthorizationHandler, TAuthorizationHandler>();

            configure?.Invoke(services);
            return services;
        }
    }
}
