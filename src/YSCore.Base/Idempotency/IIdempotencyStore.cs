namespace YSCore.Base.Idempotency;

/// <summary>
/// 幂等存储。
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// 尝试创建处理中的幂等记录。
    /// </summary>
    /// <param name="record">幂等记录。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns></returns>
    Task<IdempotencyBeginResult> TryBeginAsync(IdempotencyRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记指定的幂等记录为已完成状态。
    /// </summary>
    /// <param name="typeCode">类型</param>
    /// <param name="keyHash">哈希值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task MarkCompletedAsync(string typeCode, string keyHash, string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除指定的幂等记录。
    /// </summary>
    /// <param name="typeCode">类型</param>
    /// <param name="keyHash">哈希值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RemoveAsync(string typeCode, string keyHash, string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除已过期的幂等记录。
    /// </summary>
    /// <param name="batchSize">单次清理数量。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns></returns>
    Task<int> DeleteExpiredAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除已完成且已过期的幂等记录。
    /// </summary>
    /// <param name="batchSize">单次清理数量。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns></returns>
    Task<int> DeleteCompletedAsync(int batchSize, CancellationToken cancellationToken = default);
}
