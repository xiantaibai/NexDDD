using NexDDD.Core.Diagnostics;
using NexDDD.Observability.Abstractions;
using NexDDD.Observability.Extensions;

namespace ObservabilityDemoApi;

/// <summary>
/// 负责模拟订单处理流程，为 Demo 生成结构化日志、嵌套链路、成功指标和异常指标。
/// </summary>
public sealed class ObservabilityDemoService(
    INexObservability observability,
    ObservabilityDemoMetrics metrics,
    ILogger<ObservabilityDemoService> logger)
{
    /// <summary>
    /// 模拟一次包含参数校验和库存预留步骤的订单处理操作。
    /// </summary>
    public async Task<string> ProcessOrderAsync(
        string orderId,
        bool throwException,
        CancellationToken cancellationToken)
    {
        using var operation = observability.StartOperation(
            "demo.order.process",
            tags:
            [
                new("order.id", orderId),
                new("order.channel", "demo-api")
            ]);
        using var logScope = logger.BeginBusinessScope(
            "demo.order.process",
            new Dictionary<string, object?>
            {
                ["order.id"] = orderId,
                ["order.channel"] = "demo-api"
            });

        try
        {
            logger.LogInformation("开始处理演示订单 {OrderId}", orderId);
            await ValidateOrderAsync(orderId, cancellationToken);
            await ReserveInventoryAsync(orderId, cancellationToken);

            if (throwException)
            {
                throw new InvalidOperationException($"演示订单 {orderId} 处理失败。");
            }

            operation.SetTag("order.status", "confirmed");
            operation.SetSuccess();
            logger.LogInformation("演示订单 {OrderId} 处理完成", orderId);
            return NexDddTraceContext.GetTraceId() ?? string.Empty;
        }
        catch (Exception exception)
        {
            operation.SetError(exception);
            logger.LogError(exception, "演示订单 {OrderId} 处理失败", orderId);
            throw;
        }
    }

    /// <summary>
    /// 连续执行指定数量的业务操作，以便观察 Counter 和 Histogram 指标变化。
    /// </summary>
    public async Task GenerateMetricsAsync(
        int count,
        CancellationToken cancellationToken)
    {
        for (var index = 1; index <= count; index++)
        {
            var orderType = index % 2 == 0 ? "retail" : "wholesale";
            var startedTimestamp = metrics.OrderStarted(orderType);
            await Task.Delay(5 + index, cancellationToken);
            metrics.OrderCompleted(
                orderType,
                index % 5 == 0 ? "rejected" : "completed",
                startedTimestamp);
            metrics.SetQueueDepth(count - index);
        }

        logger.LogInformation("已独立统计 {Count} 次演示业务指标", count);
    }

    /// <summary>
    /// 输出不同级别的结构化日志，用于验证日志属性和 TraceId 关联效果。
    /// </summary>
    public void WriteStructuredLogs(string orderId)
    {
        using var operation = observability.StartOperation(
            "demo.logs.write",
            tags:
            [
                new("order.id", orderId)
            ]);
        using var logScope = logger.BeginBusinessScope(
            "demo.logs.write",
            new Dictionary<string, object?>
            {
                ["order.id"] = orderId,
                ["tenant.id"] = "demo-tenant"
            });

        logger.LogInformation(
            "订单 {OrderId} 进入日志示例，当前金额为 {Amount}",
            orderId,
            128.50m);
        logger.LogWarning(
            "订单 {OrderId} 使用演示库存告警，剩余数量为 {Remaining}",
            orderId,
            3);
        operation.SetSuccess();
    }

    private async Task ValidateOrderAsync(
        string orderId,
        CancellationToken cancellationToken)
    {
        using var operation = observability.StartOperation(
            "demo.order.validate",
            tags:
            [
                new("order.id", orderId)
            ]);

        await Task.Delay(15, cancellationToken);
        operation.SetSuccess();
    }

    private async Task ReserveInventoryAsync(
        string orderId,
        CancellationToken cancellationToken)
    {
        using var operation = observability.StartOperation(
            "demo.inventory.reserve",
            tags:
            [
                new("order.id", orderId),
                new("inventory.warehouse", "WH-DEMO-01")
            ]);

        await Task.Delay(20, cancellationToken);
        operation.SetSuccess();
    }
}
