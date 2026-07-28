using NexDDD.Core.DistributedLocks;

namespace NexDDD.Core.CommandLocks;

/// <summary>
/// 在释放锁后，从持有者集合中移除key
/// 负责命令并发控制模块中的 LockSynchronizationHandlerWarpper 能力，并限定其与调用方及基础设施组件的协作边界。
/// </summary>
/// <param name="key"></param>
/// <param name="holder"></param>
/// <param name="handler"></param>
class LockSynchronizationHandlerWarpper(string key, HashSet<string> holder, ILockSynchronizationHandler handler)
    : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await handler.DisposeAsync();
        holder.Remove(key);
    }
}