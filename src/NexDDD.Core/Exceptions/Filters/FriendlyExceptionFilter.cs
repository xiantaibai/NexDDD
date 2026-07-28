using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using NexDDD.Core.DataValidation;
using NexDDD.Core.Exceptions.Options;
using NexDDD.Core.UnifyResult;
using NexDDD.Core.Exceptions.Handlers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using NexDDD.Core.UnifyResult.Options;
using NexDDD.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexDDD.Core.DataValidation.Filters;

namespace NexDDD.Core.Exceptions.Filters
{
    /// <summary>
    /// 友好异常拦截器
    /// 负责友好异常模块中的 FriendlyExceptionFilter 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    public sealed class FriendlyExceptionFilter : IAsyncExceptionFilter
    {
        /// <summary>
        /// 异常拦截
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            // 解析异常信息
            var exceptionMetadata = UnifyContext.GetExceptionMetadata(context);

            // 判断是否是验证异常
            var isValidationException = exceptionMetadata.Error?.ValidationException == true;

            // 只有不是验证异常才处理
            if (!isValidationException)
            {
                // 解析异常处理服务，实现自定义异常额外操作，如记录日志等
                var globalExceptionHandlers = context.HttpContext.RequestServices.GetServices<IGlobalExceptionHandler>();
                foreach (var globalExceptionHandler in globalExceptionHandlers)
                {
                    await globalExceptionHandler.OnExceptionAsync(context);
                }
            }

            // 排除 WebSocket 请求处理
            if (context.HttpContext.IsWebSocketRequest()) return;

            // 如果异常在其他地方被标记了处理，那么这里不再处理
            if (context.ExceptionHandled) return;

            // 判断是否是 Razor Pages
            var isPageDescriptor = context.ActionDescriptor is CompiledPageActionDescriptor;

           

            // 处理 MVC/WebApi 请求。
            // 获取控制器信息
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            // 判断是否跳过规范化结果，如果是，则只处理为友好异常消息
            if (UnifyContext.CheckFailedNonUnify(actionDescriptor.MethodInfo, out var unifyResult))
            {
                // WebAPI 情况
                // 返回 JsonResult
                context.Result = new JsonResult(exceptionMetadata.PublicError ?? exceptionMetadata.Errors)
                {
                    StatusCode = exceptionMetadata.StatusCode,
                };
            }
            else
            {
                // 判断是否支持 MVC 规范化处理
                if (!UnifyContext.CheckSupportMvcController(context.HttpContext, actionDescriptor, out _)
                    || UnifyContext.CheckHttpContextNonUnify(context.HttpContext)) return;

                // 判断是否是验证异常，如果是，则不处理
                if (isValidationException)
                {
                    var resultHttpContext = context.HttpContext.Items[nameof(DataValidationFilter) + nameof(AppFriendlyException)];
                    // 读取验证执行结果
                    if (resultHttpContext != null)
                    {
                        // context.Result = new BadRequestObjectResult(resultHttpContext);
                        // 解析验证消息
                        var validationMetadata = ValidatorContext.GetValidationMetadata(resultHttpContext);
                        var unifyResultSettingsOptions = context.HttpContext.RequestServices.GetService<IOptions<UnifyResultSettingsOptions>>()?.Value;
                        validationMetadata.SingleValidationErrorDisplay = unifyResultSettingsOptions.SingleValidationErrorDisplay ?? false;

                        var result = unifyResult.OnValidateFailed(context, validationMetadata);
                        if (result != null) context.Result = result;

                        // 标记验证异常已被处理
                        context.ExceptionHandled = true;
                        return;
                    }
                }

                // 执行规范化异常处理
                context.Result = unifyResult.OnException(context, exceptionMetadata);
            }

            // 读取异常配置
            var friendlyExceptionSettings = context.HttpContext.RequestServices.GetRequiredService<IOptions<FriendlyExceptionSettingsOptions>>();

            // 判断是否启用异常日志输出
            if (friendlyExceptionSettings.Value.LogError == true)
            {
                // 创建日志记录器
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<AppFriendlyException>>();

                // 记录拦截日常
                logger.LogError(context.Exception,
                    "Unhandled exception. traceId:{TraceId}, code:{ErrorCode}, statusCode:{StatusCode}, path:{Path}",
                    exceptionMetadata.TraceId,
                    exceptionMetadata.Error?.Code ?? exceptionMetadata.ErrorCode,
                    exceptionMetadata.StatusCode,
                    context.HttpContext.Request.Path);
            }
        }
    }
}
