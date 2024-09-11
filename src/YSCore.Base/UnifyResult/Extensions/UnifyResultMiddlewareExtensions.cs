using Microsoft.AspNetCore.Builder;
using YSCore.Base.UnifyResult.Middlewares;

namespace YSCore.Base.UnifyResult.Extensions
{
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
