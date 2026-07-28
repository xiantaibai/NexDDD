using ObservabilityDemoApi;
using NexDDD.Core.Diagnostics;
using NexDDD.Observability.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexDddObservability();
builder.Services.AddHttpClient("observability-demo");
builder.Services.AddSingleton<ObservabilityDemoMetrics>();
builder.Services.AddScoped<ObservabilityDemoService>();
builder.Services.AddHostedService<ObservabilityDemoMetricWorker>();

var app = builder.Build();

app.MapGet(
    "/",
    () => Results.Ok(
        new
        {
            service = "NexDDD Observability Demo",
            examples = new[]
            {
                "GET /observability",
                "GET /observability/success?orderId=DEMO-001",
                "GET /observability/error?orderId=DEMO-ERROR",
                "GET /observability/logs?orderId=DEMO-LOG",
                "GET /observability/metrics?count=10",
                "GET /observability/http-client"
            }
        }));

app.MapGet(
    "/observability",
    async (
        ObservabilityDemoService demoService,
        CancellationToken cancellationToken) =>
    {
        var traceId = await demoService.ProcessOrderAsync(
            "DEMO-001",
            false,
            cancellationToken);
        return Results.Ok(
            new
            {
                message = "已输出日志、链路和指标",
                traceId
            });
    });

app.MapGet(
    "/observability/success",
    async (
        string? orderId,
        ObservabilityDemoService demoService,
        CancellationToken cancellationToken) =>
    {
        var actualOrderId = string.IsNullOrWhiteSpace(orderId)
            ? "DEMO-001"
            : orderId;
        var traceId = await demoService.ProcessOrderAsync(
            actualOrderId,
            false,
            cancellationToken);
        return Results.Ok(new { orderId = actualOrderId, traceId });
    });

app.MapGet(
    "/observability/error",
    async (
        string? orderId,
        ObservabilityDemoService demoService,
        CancellationToken cancellationToken) =>
    {
        var actualOrderId = string.IsNullOrWhiteSpace(orderId)
            ? "DEMO-ERROR"
            : orderId;

        try
        {
            await demoService.ProcessOrderAsync(
                actualOrderId,
                true,
                cancellationToken);
            return Results.Ok();
        }
        catch (InvalidOperationException exception)
        {
            return Results.Problem(
                title: "演示异常已记录",
                detail: exception.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?>
                {
                    ["traceId"] = NexDddTraceContext.GetTraceId()
                });
        }
    });

app.MapGet(
    "/observability/logs",
    (
        string? orderId,
        ObservabilityDemoService demoService) =>
    {
        var actualOrderId = string.IsNullOrWhiteSpace(orderId)
            ? "DEMO-LOG"
            : orderId;
        demoService.WriteStructuredLogs(actualOrderId);
        return Results.Ok(
            new
            {
                message = "已输出 Information 和 Warning 结构化日志",
                traceId = NexDddTraceContext.GetTraceId()
            });
    });

app.MapGet(
    "/observability/metrics",
    async (
        int? count,
        ObservabilityDemoService demoService,
        CancellationToken cancellationToken) =>
    {
        var actualCount = Math.Clamp(count ?? 10, 1, 100);
        await demoService.GenerateMetricsAsync(actualCount, cancellationToken);
        return Results.Ok(
            new
            {
                operationCount = actualCount,
                metrics = new[]
                {
                    "nexddd.demo.orders.processed",
                    "nexddd.demo.orders.active",
                    "nexddd.demo.order.processing.duration",
                    "nexddd.demo.order.queue.depth",
                    "nexddd.demo.background.samples"
                },
                createsBusinessTrace = false,
                traceId = NexDddTraceContext.GetTraceId()
            });
    });

app.MapGet(
    "/observability/http-client",
    async (
        HttpRequest request,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken) =>
    {
        var client = httpClientFactory.CreateClient("observability-demo");
        var downstreamUri = new Uri(
            $"{request.Scheme}://{request.Host}/observability/downstream");
        var response = await client.GetAsync(downstreamUri, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return Results.Ok(
            new
            {
                downstreamStatus = (int)response.StatusCode,
                downstreamContent = content,
                traceId = NexDddTraceContext.GetTraceId()
            });
    });

app.MapGet(
    "/observability/downstream",
    (ILogger<Program> logger) =>
    {
        logger.LogInformation("演示下游 HTTP 接口已收到请求");
        return Results.Ok(
            new
            {
                message = "downstream completed",
                traceId = NexDddTraceContext.GetTraceId()
            });
    });

app.Run();

/// <summary>
/// 作为可观测性示例应用的入口类型，并支持测试宿主发现该应用。
/// 用于验证可观测性相关行为，确保公开契约和运行结果保持正确。
/// </summary>
public partial class Program;
