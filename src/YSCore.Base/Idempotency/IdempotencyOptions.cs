namespace YSCore.Base.Idempotency;

/// <summary>
/// 幂等全局配置。
/// </summary>
public sealed class IdempotencyOptions
{
    /// <summary>
    /// 存储键前缀。
    /// </summary>
    public string KeyPrefix { get; set; } = "idempotency";

    /// <summary>
    /// 默认过期时长。
    /// 数据库存储建议按小时或天配置，Redis 可按分钟配置。
    /// </summary>
    public TimeSpan DefaultExpire { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// 默认过期秒数。
    /// 仅为兼容旧代码保留，新的调用请改用 <see cref="DefaultExpire"/>。
    /// </summary>
    [Obsolete("请改用 DefaultExpire 配置过期时长。")]
    public int DefaultExpireSeconds
    {
        get => (int)Math.Ceiling(DefaultExpire.TotalSeconds);
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "默认过期秒数必须大于 0。");
            }

            DefaultExpire = TimeSpan.FromSeconds(value);
        }
    }

    /// <summary>
    /// 过期数据清理间隔。
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// 已完成数据清理间隔。
    /// </summary>
    public TimeSpan CompletedCleanupInterval { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// 单次清理批量大小。
    /// </summary>
    public int CleanupBatchSize { get; set; } = 500;
}
