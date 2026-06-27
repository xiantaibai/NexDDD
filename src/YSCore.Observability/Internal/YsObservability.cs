using System.Diagnostics;
using System.Diagnostics.Metrics;
using YSCore.Observability.Abstractions;

namespace YSCore.Observability.Internal;

/// <summary>
/// 负责创建业务 Activity 和 Meter，并为业务系统提供统一的操作埋点入口。
/// </summary>
internal sealed class YsObservability : IYsObservability, IDisposable
{
    private readonly ActivitySource _activitySource;
    private readonly Meter _meter;
    private readonly Counter<long> _executions;
    private readonly Counter<long> _errors;
    private readonly Histogram<double> _duration;

    public YsObservability()
    {
        var version = typeof(YsObservability).Assembly.GetName().Version?.ToString();

        _activitySource = new ActivitySource(
            YSCoreObservabilityConstants.BusinessActivitySourceName,
            version);
        _meter = new Meter(
            YSCoreObservabilityConstants.BusinessMeterName,
            version);

        _executions = _meter.CreateCounter<long>(
            "yscore.business.operation.executions",
            unit: "{operation}",
            description: "Number of started YSCore business operations.");
        _errors = _meter.CreateCounter<long>(
            "yscore.business.operation.errors",
            unit: "{error}",
            description: "Number of failed YSCore business operations.");
        _duration = _meter.CreateHistogram<double>(
            "yscore.business.operation.duration",
            unit: "s",
            description: "Duration of YSCore business operations.");
    }

    public IYsObservabilityOperation StartOperation(
        string operationName,
        ActivityKind kind = ActivityKind.Internal,
        IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var activity = _activitySource.StartActivity(operationName, kind);
        activity?.SetTag(YSCoreObservabilityConstants.OperationNameTag, operationName);
        activity?.SetTag(YSCoreObservabilityConstants.OperationCategoryTag, "business");

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                activity?.SetTag(tag.Key, tag.Value);
            }
        }

        _executions.Add(
            1,
            new KeyValuePair<string, object?>(
                YSCoreObservabilityConstants.OperationNameTag,
                operationName));

        return new YsObservabilityOperation(
            operationName,
            activity,
            _errors,
            _duration);
    }

    public void Dispose()
    {
        _activitySource.Dispose();
        _meter.Dispose();
    }
}
