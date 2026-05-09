namespace YSCore.Base.Idempotency;

/// <summary>
/// 幂等记录状态。
/// </summary>
public enum IdempotencyStatus
{
    /// <summary>
    /// 处理中。
    /// </summary>
    Processing = 1,

    /// <summary>
    /// 已完成。
    /// </summary>
    Completed = 2
}
