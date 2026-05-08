using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using YSCore.Base.DependencyInjection.Dependencies;
using YSCore.Base.Exceptions;
using YSCore.Base.Exceptions.Handlers;
using YSCore.Base.UnifyResult;

namespace ExceptionDemoApi;

public sealed class DemoGlobalExceptionHandler(ILogger<DemoGlobalExceptionHandler> logger) : IGlobalExceptionHandler, ISingleton
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        logger.LogError(context.Exception,
            "Demo exception captured. traceId:{TraceId}, path:{Path}",
            context.HttpContext.TraceIdentifier,
            context.HttpContext.Request.Path);

        // 2. 可以在这里对特定异常进行特殊处理，例如发送通知、记录日志等

        var ex = context.Exception;
        if (ex is AppFriendlyException fe && fe.ErrorCode.ToString() == "demo.business")
        {
            context.Result = new JsonResult(new
            {
                code = 1001,
                msg = "商品库存不足，请稍后再试"
            })
            {
                StatusCode = 400
            };

            context.ExceptionHandled = true;
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
