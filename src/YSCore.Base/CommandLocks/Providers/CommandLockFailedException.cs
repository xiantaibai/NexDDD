using YSCore.Base.Exceptions;
using YSCore.Base.Idempotency;

namespace YSCore.Base.CommandLocks;

/// <summary>
/// 命令锁冲突异常。
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
