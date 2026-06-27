using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace YSCore.Base.Diagnostics;

/// <summary>
/// 统一解析当前执行上下文中的 TraceId 和 SpanId，并为未启用 Activity 的旧应用提供 HTTP 请求标识回退。
/// </summary>
public static class YSCoreTraceContext
{
    /// <summary>
    /// 获取当前 OpenTelemetry 链路标识；没有有效 Activity 时回退到 HTTP 请求标识。
    /// </summary>
    public static string? GetTraceId(HttpContext? httpContext = null)
    {
        var traceId = Activity.Current?.TraceId ?? default;
        return traceId != default
            ? traceId.ToString()
            : httpContext?.TraceIdentifier;
    }

    /// <summary>
    /// 获取当前 OpenTelemetry Span 标识；没有有效 Activity 时返回空值。
    /// </summary>
    public static string? GetSpanId()
    {
        var spanId = Activity.Current?.SpanId ?? default;
        return spanId != default
            ? spanId.ToString()
            : null;
    }
}
