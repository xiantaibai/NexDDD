using Microsoft.AspNetCore.Builder;
using NexDDD.Core.UnifyResult.Middlewares;

namespace NexDDD.Core.UnifyResult.Extensions
{
    /// <summary>
    /// 提供统一响应模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
    public static class UnifyResultMiddlewareExtensions
    {
        /// <summary>
        /// 添加状态码拦截中间件
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseUnifyResultStatusCodes(this IApplicationBuilder builder)
        {
            // 注册中间件
            UnifyContext.EnabledStatusCodesMiddleware = true;   // 设置标识
            builder.UseMiddleware<UnifyResultStatusCodesMiddleware>();

            return builder;
        }
    }
}
