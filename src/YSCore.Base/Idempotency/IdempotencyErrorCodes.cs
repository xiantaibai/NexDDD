namespace YSCore.Base.Idempotency;

/// <summary>
/// 幂等与命令锁固定错误码。
/// </summary>
public static class IdempotencyErrorCodes
{
    /// <summary>
    /// 重复提交。
    /// </summary>
    public const string RepeatSubmit = "common.idempotency.repeat";

    /// <summary>
    /// 命令锁冲突。
    /// </summary>
    public const string CommandLocked = "common.command.locked";
}
