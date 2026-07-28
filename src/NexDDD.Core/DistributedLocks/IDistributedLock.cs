namespace NexDDD.Core.DistributedLocks;

/// <summary>
/// 定义分布式锁模块的协作契约，约束调用方与具体实现之间的依赖边界。
/// </summary>
public interface IDistributedLock
{
    ILockSynchronizationHandler? TryAcquire(
        string key,
        TimeSpan timeout = default(TimeSpan),
        CancellationToken cancellationToken = default(CancellationToken));

    ILockSynchronizationHandler Acquire(
        string key,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default(CancellationToken));

    ValueTask<ILockSynchronizationHandler?> TryAcquireAsync(string key,
        TimeSpan timeout = default(TimeSpan),
        CancellationToken cancellationToken = default(CancellationToken));

    ValueTask<ILockSynchronizationHandler> AcquireAsync(string key,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default(CancellationToken));
}