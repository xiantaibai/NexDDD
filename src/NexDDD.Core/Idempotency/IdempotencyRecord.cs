namespace NexDDD.Core.Idempotency;

/// <summary>
/// 幂等记录。
/// </summary>
public sealed class IdempotencyRecord
{
    /// <summary>
    /// 业务类型编码。
    /// </summary>
    public string TypeCode { get; set; }

    /// <summary>
    /// 原始业务键。
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// 原始业务键哈希值。
    /// </summary>
    public string KeyHash { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 过期时间。
    /// </summary>
    public DateTime ExpireTime { get; set; }

    /// <summary>
    /// 当前状态。
    /// </summary>
    public IdempotencyStatus Status { get; set; }
}
