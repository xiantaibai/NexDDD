namespace ObservabilityDemoApi;

/// <summary>
/// 负责在没有 HTTP 请求和业务 Trace 的后台环境中周期生成独立指标，验证 Metric 管线能够单独工作。
/// </summary>
public sealed class ObservabilityDemoMetricWorker(
    ObservabilityDemoMetrics metrics,
    ILogger<ObservabilityDemoMetricWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sequence = 0L;

        while (!stoppingToken.IsCancellationRequested)
        {
            sequence++;
            var orderType = sequence % 2 == 0
                ? "background-retail"
                : "background-wholesale";
            metrics.SetQueueDepth(sequence % 10);
            metrics.RecordBackgroundSample();

            var startedTimestamp = metrics.OrderStarted(orderType);
            await Task.Delay(20, stoppingToken);
            metrics.OrderCompleted(
                orderType,
                "completed",
                startedTimestamp);

            logger.LogDebug("已完成第 {Sequence} 次后台独立指标采样", sequence);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
