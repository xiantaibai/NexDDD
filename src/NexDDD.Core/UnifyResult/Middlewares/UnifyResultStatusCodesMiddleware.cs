using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NexDDD.Core.Extensions;
using NexDDD.Core.UnifyResult.Options;

namespace NexDDD.Core.UnifyResult.Middlewares
{
    /// <summary>
    /// 状态码中间件
    /// 负责统一响应模块中的 UnifyResultStatusCodesMiddleware 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    public class UnifyResultStatusCodesMiddleware
    {
        /// <summary>
        /// 请求委托
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next"></param>
        public UnifyResultStatusCodesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 中间件执行方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // 只有请求错误（短路状态码）和非 WebSocket 才支持规范化处理
            if (context.IsWebSocketRequest()
                || context.Response.StatusCode < 400
                || context.Response.StatusCode == 404) return;

            // 处理规范化结果
            if (!UnifyContext.CheckExceptionHttpContextNonUnify(context, out var unifyResult))
            {
                // 解决刷新 Token 时间和 Token 时间相近问题
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized
                    && context.Response.Headers.ContainsKey("access-token")
                    && context.Response.Headers.ContainsKey("x-access-token"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                }

                // 如果 Response 已经完成输出，则禁止写入
                if (context.Response.HasStarted) return;

                var statusCode = context.Response.StatusCode;

                // 获取授权失败设置的状态码
                //var authorizationFailStatusCode = context.Items[AuthorizationHandlerContextExtensions.FAIL_STATUSCODE_KEY];
                //if (authorizationFailStatusCode != null)
                //{
                //    statusCode = Convert.ToInt32(authorizationFailStatusCode);
                //}

                await unifyResult.OnResponseStatusCodes(context, statusCode, context.RequestServices.GetService<IOptions<UnifyResultSettingsOptions>>()?.Value);
            }
        }
    }
}
