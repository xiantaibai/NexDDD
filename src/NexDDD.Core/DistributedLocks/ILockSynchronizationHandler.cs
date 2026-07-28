namespace NexDDD.Core.DistributedLocks;

/// <summary>
/// 定义分布式锁模块的协作契约，约束调用方与具体实现之间的依赖边界。
/// </summary>
public interface ILockSynchronizationHandler : IDisposable, IAsyncDisposable
{
    CancellationToken HandleLostToken { get; }
}