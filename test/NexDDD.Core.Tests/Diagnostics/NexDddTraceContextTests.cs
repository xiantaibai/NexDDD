using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NexDDD.Core.Diagnostics;
using NexDDD.Core.UnifyResult.Internal;
using NexDDD.Core.UnifyResult.Providers;

namespace NexDDD.Core.Tests.Diagnostics;

/// <summary>
/// 验证统一 Trace Context 对 Activity 链路、HTTP 回退和统一响应的解析行为。
/// </summary>
public sealed class NexDddTraceContextTests
{
    [Fact]
    public void GetTraceId_WhenActivityExists_ReturnsActivityTraceId()
    {
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "http-request-id"
        };
        using var activity = StartActivity("root");

        var traceId = NexDddTraceContext.GetTraceId(httpContext);

        Assert.Equal(activity.TraceId.ToString(), traceId);
        Assert.NotEqual(httpContext.TraceIdentifier, traceId);
    }

    [Fact]
    public void GetTraceId_WhenActivityMissing_ReturnsHttpTraceIdentifier()
    {
        var previousActivity = Activity.Current;
        Activity.Current = null;

        try
        {
            var httpContext = new DefaultHttpContext
            {
                TraceIdentifier = "http-request-id"
            };

            var traceId = NexDddTraceContext.GetTraceId(httpContext);

            Assert.Equal(httpContext.TraceIdentifier, traceId);
            Assert.Null(NexDddTraceContext.GetSpanId());
        }
        finally
        {
            Activity.Current = previousActivity;
        }
    }

    [Fact]
    public void GetTraceContext_WhenChildActivityExists_SharesTraceIdAndChangesSpanId()
    {
        using var rootActivity = StartActivity("root");
        var rootTraceId = NexDddTraceContext.GetTraceId();
        var rootSpanId = NexDddTraceContext.GetSpanId();

        using var childActivity = StartActivity("child");
        var childTraceId = NexDddTraceContext.GetTraceId();
        var childSpanId = NexDddTraceContext.GetSpanId();

        Assert.Equal(rootTraceId, childTraceId);
        Assert.NotEqual(rootSpanId, childSpanId);
    }

    [Fact]
    public void OnSucceeded_WhenActivityExists_ReturnsActivityTraceId()
    {
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "http-request-id"
        };
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());
        var executedContext = new ActionExecutedContext(
            actionContext,
            [],
            new object());
        var provider = new RESTfulResultProvider();
        using var activity = StartActivity("request");

        var result = Assert.IsType<JsonResult>(
            provider.OnSucceeded(executedContext, new { Value = 1 }));
        var response = Assert.IsType<AppRESTfulResult<object>>(result.Value);

        Assert.Equal(activity.TraceId.ToString(), response.TraceId);
    }

    private static Activity StartActivity(string operationName)
    {
        var activity = new Activity(operationName);
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();
        return activity;
    }
}
