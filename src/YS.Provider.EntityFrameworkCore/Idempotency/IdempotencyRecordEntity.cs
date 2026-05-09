using Yitter.IdGenerator;
using YSCore.Base.DatabaseAccessor;
using YSCore.Base.Idempotency;

namespace YSCore.Provider.EntityFrameworkCore.Idempotency;

/// <summary>
/// 幂等记录实体。
/// </summary>
public sealed class IdempotencyRecordEntity : Entity<long>, IAggregateRoot
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
    /// 当前执行实例ID
    /// 用于防止超时恢复后的状态污染
    /// </summary>
    public string RequestId { get; set; }

    /// <summary>
    /// 过期时间。
    /// </summary>
    public DateTime ExpireTime { get; set; }

    /// <summary>
    /// 当前状态。
    /// </summary>
    public IdempotencyStatus Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 创建幂等记录实体。
    /// </summary>
    /// <param name="record">幂等记录。</param>
    /// <returns></returns>
    public static IdempotencyRecordEntity Create(IdempotencyRecord record)
    {
        var requestId = Guid.NewGuid()
            .ToString("N");

        return new IdempotencyRecordEntity
        {
            Id = YitIdHelper.NextId(),
            TypeCode = record.TypeCode,
            Key = record.Key,
            KeyHash = record.KeyHash,
            CreateTime = record.CreateTime,
            ExpireTime = record.ExpireTime,
            Status = record.Status,
            RequestId = requestId
        };
    }
}
