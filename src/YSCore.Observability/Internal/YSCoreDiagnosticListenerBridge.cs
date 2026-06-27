using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using YSCore.Base.Diagnostics;

namespace YSCore.Observability.Internal;

/// <summary>
/// 负责把现有 YSCore DiagnosticListener 事件转换成 Activity 和 Metric，
/// 在框架组件迁移到原生 OpenTelemetry API 期间保持兼容。
/// </summary>
internal sealed class YSCoreDiagnosticListenerBridge :
    IHostedService,
    IObserver<DiagnosticListener>,
    IObserver<KeyValuePair<string, object?>>,
    IDisposable
{
    private const string RedisListenerName = "YSCore.RedisLockDiagnosticListener";
    private const string RedisAcquireBegin = "YSCore.RedisLock.AcquireBegin";
    private const string RedisAcquireEnd = "YSCore.RedisLock.AcquireEnd";
    private const string RedisAcquireError = "YSCore.RedisLock.AcquireErrorData";
    private const string RedisReleaseBegin = "YSCore.RedisLock.ReleaseBegin";
    private const string RedisReleaseEnd = "YSCore.RedisLock.ReleaseEnd";
    private const string RedisReleaseError = "YSCore.RedisLock.ReleaseError";

    private readonly ConcurrentDictionary<Guid, DiagnosticOperation> _operations = new();
    private readonly List<IDisposable> _listenerSubscriptions = [];
    private readonly object _subscriptionsLock = new();
    private readonly ActivitySource _activitySource;
    private readonly Meter _meter;
    private readonly InstrumentSet _commands;
    private readonly InstrumentSet _domainEvents;
    private readonly InstrumentSet _transactions;
    private readonly InstrumentSet _distributedLocks;
    private IDisposable? _allListenersSubscription;

    public YSCoreDiagnosticListenerBridge()
    {
        var version = typeof(YSCoreDiagnosticListenerBridge).Assembly.GetName().Version?.ToString();
        _activitySource = new ActivitySource(
            YSCoreObservabilityConstants.FrameworkActivitySourceName,
            version);
        _meter = new Meter(
            YSCoreObservabilityConstants.FrameworkMeterName,
            version);

        _commands = new InstrumentSet(_meter, "yscore.command", "command");
        _domainEvents = new InstrumentSet(_meter, "yscore.domain_event", "domain event");
        _transactions = new InstrumentSet(_meter, "yscore.transaction", "transaction");
        _distributedLocks = new InstrumentSet(_meter, "yscore.distributed_lock", "distributed lock");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _allListenersSubscription = DiagnosticListener.AllListeners.Subscribe(this);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        DisposeSubscriptions();

        foreach (var operation in _operations.Values)
        {
            operation.Complete(isError: false, description: "application_stopping");
        }

        _operations.Clear();
        return Task.CompletedTask;
    }

    public void OnNext(DiagnosticListener listener)
    {
        if (listener.Name is not YsNetCoreDiagnosticListenerNames.DiagnosticListenerName
            and not RedisListenerName)
        {
            return;
        }

        var subscription = listener.Subscribe(this);
        lock (_subscriptionsLock)
        {
            _listenerSubscriptions.Add(subscription);
        }
    }

    public void OnNext(KeyValuePair<string, object?> diagnosticEvent)
    {
        switch (diagnosticEvent.Key)
        {
            case YsNetCoreDiagnosticListenerNames.CommandHandlerBegin
                when diagnosticEvent.Value is CommandBegin command:
                Begin(command.Id, "command", command.Name, _commands);
                break;

            case YsNetCoreDiagnosticListenerNames.CommandHandlerEnd
                when diagnosticEvent.Value is CommandEnd command:
                Complete(command.Id);
                break;

            case YsNetCoreDiagnosticListenerNames.CommandHandlerError
                when diagnosticEvent.Value is CommandError command:
                Complete(command.Id, command.Exception);
                break;

            case YsNetCoreDiagnosticListenerNames.DomainEventHandlerBegin
                when diagnosticEvent.Value is DomainEventHandlerBegin domainEvent:
                Begin(domainEvent.Id, "domain_event", domainEvent.Name, _domainEvents);
                break;

            case YsNetCoreDiagnosticListenerNames.DomainEventHandlerEnd
                when diagnosticEvent.Value is DomainEventHandlerEnd domainEvent:
                Complete(domainEvent.Id);
                break;

            case YsNetCoreDiagnosticListenerNames.DomainEventHandlerError
                when diagnosticEvent.Value is DomainEventHandlerError domainEvent:
                Complete(domainEvent.Id, domainEvent.Exception);
                break;

            case YsNetCoreDiagnosticListenerNames.TransactionBegin
                when diagnosticEvent.Value is Guid transactionId:
                Begin(transactionId, "transaction", "database", _transactions);
                break;

            case YsNetCoreDiagnosticListenerNames.TransactionCommit
                when diagnosticEvent.Value is Guid transactionId:
                Complete(transactionId, description: "commit");
                break;

            case YsNetCoreDiagnosticListenerNames.TransactionRollback
                when diagnosticEvent.Value is Guid transactionId:
                Complete(transactionId, isError: true, description: "rollback");
                break;

            case RedisAcquireBegin:
                BeginReflected(diagnosticEvent.Value, "distributed_lock", "acquire");
                break;

            case RedisAcquireEnd:
                CompleteReflected(diagnosticEvent.Value);
                break;

            case RedisAcquireError:
                CompleteReflected(diagnosticEvent.Value, GetException(diagnosticEvent.Value));
                break;

            case RedisReleaseBegin:
                BeginReflected(diagnosticEvent.Value, "distributed_lock", "release");
                break;

            case RedisReleaseEnd:
                CompleteReflected(diagnosticEvent.Value);
                break;

            case RedisReleaseError:
                CompleteReflected(diagnosticEvent.Value, GetException(diagnosticEvent.Value));
                break;
        }
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    private void Begin(
        Guid id,
        string category,
        string operationName,
        InstrumentSet instruments)
    {
        var activity = _activitySource.StartActivity(
            $"{category} {operationName}",
            ActivityKind.Internal);
        activity?.SetTag(YSCoreObservabilityConstants.OperationCategoryTag, category);
        activity?.SetTag(YSCoreObservabilityConstants.OperationNameTag, operationName);

        var operation = new DiagnosticOperation(
            operationName,
            activity,
            instruments);

        if (_operations.TryGetValue(id, out var previous))
        {
            previous.Complete(isError: true, description: "duplicate_operation_id");
        }

        _operations[id] = operation;
    }

    private void Complete(
        Guid id,
        Exception? exception = null,
        bool isError = false,
        string? description = null)
    {
        if (_operations.TryRemove(id, out var operation))
        {
            operation.Complete(
                isError || exception != null,
                description,
                exception);
        }
    }

    private void BeginReflected(object? payload, string category, string fallbackName)
    {
        var id = GetProperty<Guid>(payload, "Id");
        if (id == Guid.Empty)
        {
            return;
        }

        var operationName = GetProperty<string>(payload, "MethodName");
        Begin(
            id,
            category,
            string.IsNullOrWhiteSpace(operationName) ? fallbackName : operationName,
            _distributedLocks);
    }

    private void CompleteReflected(object? payload, Exception? exception = null)
    {
        var id = GetProperty<Guid>(payload, "Id");
        if (id != Guid.Empty)
        {
            Complete(id, exception);
        }
    }

    private static Exception? GetException(object? payload)
        => GetProperty<Exception>(payload, "Exception");

    private static T? GetProperty<T>(object? payload, string name)
    {
        if (payload == null)
        {
            return default;
        }

        var value = payload.GetType().GetProperty(name)?.GetValue(payload);
        return value is T result ? result : default;
    }

    private void DisposeSubscriptions()
    {
        _allListenersSubscription?.Dispose();
        _allListenersSubscription = null;

        lock (_subscriptionsLock)
        {
            foreach (var subscription in _listenerSubscriptions)
            {
                subscription.Dispose();
            }

            _listenerSubscriptions.Clear();
        }
    }

    public void Dispose()
    {
        DisposeSubscriptions();
        _activitySource.Dispose();
        _meter.Dispose();
    }

    /// <summary>
    /// 封装一类框架操作共用的执行次数、错误次数和耗时指标。
    /// </summary>
    private sealed class InstrumentSet
    {
        public InstrumentSet(Meter meter, string prefix, string displayName)
        {
            Executions = meter.CreateCounter<long>(
                $"{prefix}.executions",
                unit: "{operation}",
                description: $"Number of started YSCore {displayName} operations.");
            Errors = meter.CreateCounter<long>(
                $"{prefix}.errors",
                unit: "{error}",
                description: $"Number of failed YSCore {displayName} operations.");
            Duration = meter.CreateHistogram<double>(
                $"{prefix}.duration",
                unit: "s",
                description: $"Duration of YSCore {displayName} operations.");
        }

        public Counter<long> Executions { get; }

        public Counter<long> Errors { get; }

        public Histogram<double> Duration { get; }
    }

    /// <summary>
    /// 维护由诊断事件启动的单次框架操作，并在结束事件到达时完成链路和指标记录。
    /// </summary>
    private sealed class DiagnosticOperation
    {
        private readonly string _operationName;
        private readonly Activity? _activity;
        private readonly InstrumentSet _instruments;
        private readonly long _startedTimestamp = Stopwatch.GetTimestamp();
        private int _completed;

        public DiagnosticOperation(
            string operationName,
            Activity? activity,
            InstrumentSet instruments)
        {
            _operationName = operationName;
            _activity = activity;
            _instruments = instruments;
            _instruments.Executions.Add(
                1,
                new KeyValuePair<string, object?>(
                    YSCoreObservabilityConstants.OperationNameTag,
                    _operationName));
        }

        public void Complete(
            bool isError,
            string? description = null,
            Exception? exception = null)
        {
            if (Interlocked.Exchange(ref _completed, 1) != 0)
            {
                return;
            }

            var status = isError ? "error" : "ok";
            _activity?.SetTag(YSCoreObservabilityConstants.OperationStatusTag, status);
            _activity?.SetStatus(
                isError ? ActivityStatusCode.Error : ActivityStatusCode.Ok,
                description);

            if (exception != null)
            {
                _activity?.AddEvent(
                    new ActivityEvent(
                        "exception",
                        tags: new ActivityTagsCollection
                        {
                            ["exception.type"] = exception.GetType().FullName,
                            ["exception.message"] = exception.Message
                        }));
            }

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

            if (isError)
            {
                _instruments.Errors.Add(1, tags);
            }

            _instruments.Duration.Record(
                Stopwatch.GetElapsedTime(_startedTimestamp).TotalSeconds,
                tags);
            _activity?.Dispose();
        }
    }
}
