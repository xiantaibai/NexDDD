using Microsoft.AspNetCore.Mvc.Filters;
using YSCore.Base.Exceptions.Handlers;

namespace ExceptionDemoApi;

public sealed class DemoGlobalExceptionHandler(ILogger<DemoGlobalExceptionHandler> logger) : IGlobalExceptionHandler
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        logger.LogError(context.Exception,
            "Demo exception captured. traceId:{TraceId}, path:{Path}",
            context.HttpContext.TraceIdentifier,
            context.HttpContext.Request.Path);

        return Task.CompletedTask;
    }
}
