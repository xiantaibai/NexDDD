using System.Diagnostics;
using System.Diagnostics.Metrics;
using YSCore.Observability.Abstractions;

namespace YSCore.Observability.Internal;

/// <summary>
/// 负责维护单次业务操作的链路状态，并在结束时记录耗时和执行结果指标。
/// </summary>
internal sealed class YsObservabilityOperation : IYsObservabilityOperation
{
    private readonly string _operationName;
    private readonly Counter<long> _errors;
    private readonly Histogram<double> _duration;
    private readonly long _startedTimestamp = Stopwatch.GetTimestamp();
    private int _outcome;
    private int _disposed;

    public YsObservabilityOperation(
        string operationName,
        Activity? activity,
        Counter<long> errors,
        Histogram<double> duration)
    {
        _operationName = operationName;
        Activity = activity;
        _errors = errors;
        _duration = duration;
    }

    public Activity? Activity { get; }

    public IYsObservabilityOperation SetTag(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Activity?.SetTag(name, value);
        return this;
    }

    public void SetSuccess()
    {
        var previousOutcome = Interlocked.CompareExchange(ref _outcome, 1, 0);
        if (previousOutcome is 0 or 1)
        {
            Activity?.SetStatus(ActivityStatusCode.Ok);
        }
    }

    public void SetError(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var previousOutcome = Interlocked.Exchange(ref _outcome, 2);
        Activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
        Activity?.AddEvent(
            new ActivityEvent(
                "exception",
                tags: new ActivityTagsCollection
                {
                    ["exception.type"] = exception.GetType().FullName,
                    ["exception.message"] = exception.Message
                }));

        if (previousOutcome != 2)
        {
            _errors.Add(
                1,
                new KeyValuePair<string, object?>(
                    YSCoreObservabilityConstants.OperationNameTag,
                    _operationName));
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        var outcome = Volatile.Read(ref _outcome);
        var status = outcome switch
        {
            1 => "ok",
            2 => "error",
            _ => "unset"
        };

        var tags = new TagList
        {
            {
                YSCoreObservabilityConstants.OperationNameTag,
                _operationName
            },
            {
                YSCoreObservabilityConstants.OperationStatusTag,
                status
            }
        };

        _duration.Record(
            Stopwatch.GetElapsedTime(_startedTimestamp).TotalSeconds,
            tags);
        Activity?.Dispose();
    }
}
