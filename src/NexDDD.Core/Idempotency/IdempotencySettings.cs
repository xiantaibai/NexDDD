namespace NexDDD.Core.Idempotency;

/// <summary>
/// 幂等配置项。
/// 承载幂等执行流程中的不可变数据，供处理器和基础设施组件安全传递。
/// </summary>
public sealed record IdempotencySettings
{
    /// <summary>
    /// 构造幂等配置项。
    /// 使用全局默认过期时间。
    /// </summary>
    /// <param name="typeCode">业务类型编码。</param>
    /// <param name="key">业务幂等键。</param>
    public IdempotencySettings(string typeCode, string key)
        : this(typeCode, key, null)
    {
    }

    /// <summary>
    /// 构造幂等配置项。
    /// </summary>
    /// <param name="typeCode">业务类型编码。</param>
    /// <param name="key">业务幂等键。</param>
    /// <param name="expireIn">过期时长。</param>
    /// <exception cref="ArgumentException"></exception>
    public IdempotencySettings(string typeCode, string key, TimeSpan? expireIn)
    {
        if (string.IsNullOrWhiteSpace(typeCode))
        {
            throw new ArgumentException("业务类型不能为空。", nameof(typeCode));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("幂等键不能为空。", nameof(key));
        }

        if (expireIn.HasValue && expireIn.Value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(expireIn), "过期时长必须大于 0。");
        }

        TypeCode = typeCode.Trim();
        Key = key.Trim();
        ExpireIn = expireIn;
    }

    /// <summary>
    /// 构造幂等配置项。
    /// 仅为兼容旧代码保留，新的调用请改用 <see cref="IdempotencySettings(string, string, TimeSpan?)"/>。
    /// </summary>
    /// <param name="typeCode">业务类型编码。</param>
    /// <param name="key">业务幂等键。</param>
    /// <param name="expireSeconds">过期秒数。</param>
    [Obsolete("请改用 TimeSpan 方式配置过期时长。")]
    public IdempotencySettings(string typeCode, string key, int expireSeconds)
        : this(typeCode, key, TimeSpan.FromSeconds(expireSeconds))
    {
    }

    /// <summary>
    /// 业务类型编码。
    /// </summary>
    public string TypeCode { get; }

    /// <summary>
    /// 原始业务键。
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 过期时长。
    /// 为空时使用全局默认值。
    /// </summary>
    public TimeSpan? ExpireIn { get; }
}
