using YSCore.Base.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;

namespace YSCore.Base.AutherUserContext
{
    public static class AutherUserContextExtensions
    {
        /// <summary>
        /// 添加用户授权上下文
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAutherUserContex(this IServiceCollection services)
        {
           services.AddContext<AutherUserContext>(options =>
           {
               options.ContextFactory = new AutherUserContextFactory(services.BuildServiceProvider().GetService<IHttpContextAccessor>());
           });

            return services;
        }
    }
}