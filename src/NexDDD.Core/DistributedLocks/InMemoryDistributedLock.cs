using AsyncKeyedLock;

namespace NexDDD.Core.DistributedLocks;

/// <summary>
/// 提供面向测试和单进程场景的内存分布式锁实现。
/// 锁状态仅存在于当前进程，不适用于多进程或多节点部署。
/// </summary>
public sealed class InMemoryDistributedLock : IDistributedLock
{
    private static readonly AsyncKeyedLocker<string> Locks = new();

    public ILockSynchronizationHandler? TryAcquire(string key, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        // 默认 TimeSpan 表示立即尝试且不等待。
        var waitTime = timeout == default ? TimeSpan.Zero : timeout;
        var acquired = Locks.LockOrNull(key, waitTime, cancellationToken);
        return acquired is null ? null : new Handle(acquired);
    }

    public ILockSynchronizationHandler Acquire(string key, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        if (timeout is null)
        {
            var semLock = Locks.Lock(key, cancellationToken);
            return new Handle(semLock);
        }

        var timeoutLock = Locks.LockOrNull(key, timeout.Value, cancellationToken)
            ?? throw new TimeoutException($"Failed to acquire lock for key '{key}' within {timeout.Value}.");

        return new Handle(timeoutLock);
    }

    public async ValueTask<ILockSynchronizationHandler?> TryAcquireAsync(string key, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        // 默认 TimeSpan 表示立即尝试且不等待。
        var waitTime = timeout == default ? TimeSpan.Zero : timeout;
        var acquired = await Locks.LockOrNullAsync(key, waitTime, cancellationToken).ConfigureAwait(false);
        return acquired is null ? null : new Handle(acquired);
    }

    public async ValueTask<ILockSynchronizationHandler> AcquireAsync(string key, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        if (timeout is null)
        {
            var semLock = await Locks.LockAsync(key, cancellationToken).ConfigureAwait(false);
            return new Handle(semLock);
        }

        var timeoutLock = await Locks.LockOrNullAsync(key, timeout.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new TimeoutException($"Failed to acquire lock for key '{key}' within {timeout.Value}.");
        
        return new Handle(timeoutLock);
    }

    /// <summary>
    /// 负责分布式锁模块中的 Handle 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    private sealed class Handle(IDisposable disposable) : ILockSynchronizationHandler
    {
        private readonly IDisposable? _disposable = disposable;
        private readonly CancellationTokenSource _cts = new();

        public CancellationToken HandleLostToken => _cts.Token; // 内存锁在句柄释放前不会丢失所有权。

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
