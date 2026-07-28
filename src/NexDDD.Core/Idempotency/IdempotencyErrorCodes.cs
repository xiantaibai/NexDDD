namespace NexDDD.Core.Idempotency;

/// <summary>
/// 幂等与命令锁固定错误码。
/// 负责幂等执行模块中的 IdempotencyErrorCodes 能力，并限定其与调用方及基础设施组件的协作边界。
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
