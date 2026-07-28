using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using NexDDD.Core.Diagnostics;

namespace NexDDD.Observability.Internal;

/// <summary>
/// 负责把现有 NexDDD DiagnosticListener 事件转换成 Activity 和 Metric，
/// 在框架组件迁移到原生 OpenTelemetry API 期间保持兼容。
/// </summary>
internal sealed class NexDddDiagnosticListenerBridge :
    IHostedService,
    IObserver<DiagnosticListener>,
    IObserver<KeyValuePair<string, object?>>,
    IDisposable
{
    private const string RedisListenerName = "NexDDD.RedisLockDiagnosticListener";
    private const string RedisAcquireBegin = "NexDDD.RedisLock.AcquireBegin";
    private const string RedisAcquireEnd = "NexDDD.RedisLock.AcquireEnd";
    private const string RedisAcquireError = "NexDDD.RedisLock.AcquireErrorData";
    private const string RedisReleaseBegin = "NexDDD.RedisLock.ReleaseBegin";
    private const string RedisReleaseEnd = "NexDDD.RedisLock.ReleaseEnd";
    private const string RedisReleaseError = "NexDDD.RedisLock.ReleaseError";

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

    public NexDddDiagnosticListenerBridge()
    {
        var version = typeof(NexDddDiagnosticListenerBridge).Assembly.GetName().Version?.ToString();
        _activitySource = new ActivitySource(
            NexDddObservabilityConstants.FrameworkActivitySourceName,
            version);
        _meter = new Meter(
            NexDddObservabilityConstants.FrameworkMeterName,
            version);

        _commands = new InstrumentSet(_meter, "nexddd.command", "command");
        _domainEvents = new InstrumentSet(_meter, "nexddd.domain_event", "domain event");
        _transactions = new InstrumentSet(_meter, "nexddd.transaction", "transaction");
        _distributedLocks = new InstrumentSet(_meter, "nexddd.distributed_lock", "distributed lock");
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
        if (listener.Name is not NexDddDiagnosticListenerNames.DiagnosticListenerName
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
            case NexDddDiagnosticListenerNames.CommandHandlerBegin
                when diagnosticEvent.Value is CommandBegin command:
                Begin(command.Id, "command", command.Name, _commands);
                break;

            case NexDddDiagnosticListenerNames.CommandHandlerEnd
                when diagnosticEvent.Value is CommandEnd command:
                Complete(command.Id);
                break;

            case NexDddDiagnosticListenerNames.CommandHandlerError
                when diagnosticEvent.Value is CommandError command:
                Complete(command.Id, command.Exception);
                break;

            case NexDddDiagnosticListenerNames.DomainEventHandlerBegin
                when diagnosticEvent.Value is DomainEventHandlerBegin domainEvent:
                Begin(domainEvent.Id, "domain_event", domainEvent.Name, _domainEvents);
                break;

            case NexDddDiagnosticListenerNames.DomainEventHandlerEnd
                when diagnosticEvent.Value is DomainEventHandlerEnd domainEvent:
                Complete(domainEvent.Id);
                break;

            case NexDddDiagnosticListenerNames.DomainEventHandlerError
                when diagnosticEvent.Value is DomainEventHandlerError domainEvent:
                Complete(domainEvent.Id, domainEvent.Exception);
                break;

            case NexDddDiagnosticListenerNames.TransactionBegin
                when diagnosticEvent.Value is Guid transactionId:
                Begin(transactionId, "transaction", "database", _transactions);
                break;

            case NexDddDiagnosticListenerNames.TransactionCommit
                when diagnosticEvent.Value is Guid transactionId:
                Complete(transactionId, description: "commit");
                break;

            case NexDddDiagnosticListenerNames.TransactionRollback
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
        activity?.SetTag(NexDddObservabilityConstants.OperationCategoryTag, category);
        activity?.SetTag(NexDddObservabilityConstants.OperationNameTag, operationName);

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
                description: $"Number of started NexDDD {displayName} operations.");
            Errors = meter.CreateCounter<long>(
                $"{prefix}.errors",
                unit: "{error}",
                description: $"Number of failed NexDDD {displayName} operations.");
            Duration = meter.CreateHistogram<double>(
                $"{prefix}.duration",
                unit: "s",
                description: $"Duration of NexDDD {displayName} operations.");
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
                    NexDddObservabilityConstants.OperationNameTag,
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
            _activity?.SetTag(NexDddObservabilityConstants.OperationStatusTag, status);
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
                    NexDddObservabilityConstants.OperationNameTag,
                    _operationName
                },
                {
                    NexDddObservabilityConstants.OperationStatusTag,
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
