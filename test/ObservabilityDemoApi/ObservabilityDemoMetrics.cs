using System.Diagnostics;
using System.Diagnostics.Metrics;
using YSCore.Observability;

namespace ObservabilityDemoApi;

/// <summary>
/// 负责定义并记录 Demo 的独立业务指标，不创建 Activity，也不依赖任何 Trace 才能完成统计。
/// </summary>
public sealed class ObservabilityDemoMetrics
{
    private readonly Counter<long> _ordersProcessed;
    private readonly UpDownCounter<long> _ordersActive;
    private readonly Histogram<double> _orderDuration;
    private readonly Counter<long> _backgroundSamples;
    private long _queueDepth;

    public ObservabilityDemoMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(
            YSCoreObservabilityConstants.BusinessMeterName);

        _ordersProcessed = meter.CreateCounter<long>(
            "yscore.demo.orders.processed",
            unit: "{order}",
            description: "演示订单处理总数。");
        _ordersActive = meter.CreateUpDownCounter<long>(
            "yscore.demo.orders.active",
            unit: "{order}",
            description: "当前正在处理的演示订单数量。");
        _orderDuration = meter.CreateHistogram<double>(
            "yscore.demo.order.processing.duration",
            unit: "s",
            description: "演示订单处理耗时。");
        _backgroundSamples = meter.CreateCounter<long>(
            "yscore.demo.background.samples",
            unit: "{sample}",
            description: "后台独立指标采样次数。");
        meter.CreateObservableGauge(
            "yscore.demo.order.queue.depth",
            () => Interlocked.Read(ref _queueDepth),
            unit: "{order}",
            description: "演示订单队列当前深度。");
    }

    /// <summary>
    /// 记录一个订单开始处理，并增加当前活动订单数。
    /// </summary>
    public long OrderStarted(string orderType)
    {
        _ordersActive.Add(
            1,
            new KeyValuePair<string, object?>("order.type", orderType));
        return Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// 记录订单处理结果、处理耗时，并减少当前活动订单数。
    /// </summary>
    public void OrderCompleted(
        string orderType,
        string status,
        long startedTimestamp)
    {
        var tags = new TagList
        {
            { "order.type", orderType },
            { "order.status", status }
        };

        _ordersProcessed.Add(1, tags);
        _orderDuration.Record(
            Stopwatch.GetElapsedTime(startedTimestamp).TotalSeconds,
            tags);
        _ordersActive.Add(
            -1,
            new KeyValuePair<string, object?>("order.type", orderType));
    }

    /// <summary>
    /// 更新可观察队列深度，该值会在每次 Metric 导出时独立采集。
    /// </summary>
    public void SetQueueDepth(long queueDepth)
    {
        Interlocked.Exchange(ref _queueDepth, queueDepth);
    }

    /// <summary>
    /// 记录一次没有 Trace 参与的后台指标采样。
    /// </summary>
    public void RecordBackgroundSample()
    {
        _backgroundSamples.Add(
            1,
            new KeyValuePair<string, object?>(
                "sample.source",
                "background"));
    }
}
