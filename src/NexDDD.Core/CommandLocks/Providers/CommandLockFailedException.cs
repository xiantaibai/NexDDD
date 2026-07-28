using NexDDD.Core.Exceptions;
using NexDDD.Core.Idempotency;

namespace NexDDD.Core.CommandLocks;

/// <summary>
/// 命令锁冲突异常。
/// 负责命令并发控制模块中的 CommandLockFailedException 能力，并限定其与调用方及基础设施组件的协作边界。
/// </summary>
public sealed class CommandLockFailedException : AppFriendlyException
{
    public CommandLockFailedException(string message, string failedKey)
        : base(message, IdempotencyErrorCodes.CommandLocked)
    {
        FailedKey = failedKey;
    }

    public string FailedKey { get; }
}
