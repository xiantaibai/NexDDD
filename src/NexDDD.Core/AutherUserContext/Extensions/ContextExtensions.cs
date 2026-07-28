using NexDDD.Core.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;

namespace NexDDD.Core.AutherUserContext
{
    /// <summary>
    /// 提供请求上下文模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
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