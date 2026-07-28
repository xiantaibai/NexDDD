using System.Diagnostics;
using System.Diagnostics.Metrics;
using NexDDD.Observability.Abstractions;

namespace NexDDD.Observability.Internal;

/// <summary>
/// 负责创建业务 Activity 和 Meter，并为业务系统提供统一的操作埋点入口。
/// </summary>
internal sealed class NexObservability : INexObservability, IDisposable
{
    private readonly ActivitySource _activitySource;
    private readonly Meter _meter;
    private readonly Counter<long> _executions;
    private readonly Counter<long> _errors;
    private readonly Histogram<double> _duration;

    public NexObservability()
    {
        var version = typeof(NexObservability).Assembly.GetName().Version?.ToString();

        _activitySource = new ActivitySource(
            NexDddObservabilityConstants.BusinessActivitySourceName,
            version);
        _meter = new Meter(
            NexDddObservabilityConstants.BusinessMeterName,
            version);

        _executions = _meter.CreateCounter<long>(
            "nexddd.business.operation.executions",
            unit: "{operation}",
            description: "Number of started NexDDD business operations.");
        _errors = _meter.CreateCounter<long>(
            "nexddd.business.operation.errors",
            unit: "{error}",
            description: "Number of failed NexDDD business operations.");
        _duration = _meter.CreateHistogram<double>(
            "nexddd.business.operation.duration",
            unit: "s",
            description: "Duration of NexDDD business operations.");
    }

    public INexObservabilityOperation StartOperation(
        string operationName,
        ActivityKind kind = ActivityKind.Internal,
        IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var activity = _activitySource.StartActivity(operationName, kind);
        activity?.SetTag(NexDddObservabilityConstants.OperationNameTag, operationName);
        activity?.SetTag(NexDddObservabilityConstants.OperationCategoryTag, "business");

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
                NexDddObservabilityConstants.OperationNameTag,
                operationName));

        return new NexObservabilityOperation(
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
