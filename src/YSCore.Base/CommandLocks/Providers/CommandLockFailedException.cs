namespace YSCore.Base.CommandLocks;

/// <summary>
/// 锁异常
/// </summary>
public sealed class CommandLockFailedException : Exception
{
    public CommandLockFailedException(string message, string failedKey) : base(message)
    {
        FailedKey = failedKey;
    }

    public string FailedKey { get; private set; }
}