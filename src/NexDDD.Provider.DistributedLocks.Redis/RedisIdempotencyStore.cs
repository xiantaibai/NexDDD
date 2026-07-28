using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using NexDDD.Core.Idempotency;

namespace NexDDD.Provider.DistributedLocks.Redis;

/// <summary>
/// 基于 Redis 的幂等存储。
/// 负责分布式锁模块中的 RedisIdempotencyStore 能力，并限定其与调用方及基础设施组件的协作边界。
/// </summary>
public sealed class RedisIdempotencyStore(
    IDatabase database,
    IOptions<IdempotencyOptions> options) : IIdempotencyStore
{
    public async Task<IdempotencyBeginResult> TryBeginAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
    {
        var key = IdempotencyKeyHelper.BuildStorageKey(options.Value.KeyPrefix, record.TypeCode, record.KeyHash);
        var now = DateTime.UtcNow;

        var requestId = Guid.NewGuid()
            .ToString("N");

        var redisRecord = new RedisIdempotencyRecord
        {
            TypeCode = record.TypeCode,
            KeyHash = record.KeyHash,
            RequestId = requestId,
            Status = IdempotencyStatus.Processing,
            CreateTime = now,
            LastModifyTime = now,
            ExpireTime = record.ExpireTime
        };

        var ttl = record.ExpireTime - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            ttl = TimeSpan.FromSeconds(1);
        }

        // 第一次抢占

        var inserted = await database.StringSetAsync(
            key,
            JsonSerializer.Serialize(redisRecord),
            ttl,
            When.NotExists);

        if (inserted)
        {
            return IdempotencyBeginResult
                .SuccessResult(requestId);
        }

        // 已存在
        var value = await database.StringGetAsync(key);

        if (!value.HasValue)
        {
            return IdempotencyBeginResult
                .ProcessingResult();
        }

        var current =
            JsonSerializer.Deserialize<RedisIdempotencyRecord>(
                value.ToString());

        if (current == null)
        {
            return IdempotencyBeginResult
                .ProcessingResult();
        }

        // 已完成
        if (current.Status == IdempotencyStatus.Completed)
        {
            return IdempotencyBeginResult
                .CompletedResult();
        }

        // Processing 未过期
        if (current.ExpireTime > now)
        {
            return IdempotencyBeginResult
                .ProcessingResult();
        }

        // Processing 已过期
        // 尝试抢占

        var newRecord = new RedisIdempotencyRecord
        {
            TypeCode = record.TypeCode,
            KeyHash = record.KeyHash,
            RequestId = requestId,
            Status = IdempotencyStatus.Processing,
            CreateTime = current.CreateTime,
            LastModifyTime = now,
            ExpireTime = record.ExpireTime
        };

        // CAS 抢占

        var tran = database.CreateTransaction();

        tran.AddCondition(
            Condition.StringEqual(
                key,
                value));

        _ = tran.StringSetAsync(
            key,
            JsonSerializer.Serialize(newRecord),
            ttl);

        var success = await tran.ExecuteAsync();

        if (success)
        {
            return IdempotencyBeginResult
                .SuccessResult(requestId);
        }

        return IdempotencyBeginResult
            .ProcessingResult();
    }

    public async Task MarkCompletedAsync(string typeCode, string keyHash, string requestId, CancellationToken cancellationToken = default)
    {
        var key = IdempotencyKeyHelper.BuildStorageKey(
             options.Value.KeyPrefix,
             typeCode,
             keyHash);

        var value = await database.StringGetAsync(key);

        if (!value.HasValue)
        {
            return;
        }

        var record =
            JsonSerializer.Deserialize<RedisIdempotencyRecord>(
                value.ToString());

        if (record == null)
        {
            return;
        }

        // requestId 不一致
        // 说明锁已经被其他请求抢占

        if (!string.Equals(
                record.RequestId,
                requestId,
                StringComparison.Ordinal))
        {
            return;
        }

        // 已完成
        // 跳过

        if (record.Status == IdempotencyStatus.Completed)
        {
            return;
        }

        record.Status =
            IdempotencyStatus.Completed;

        record.LastModifyTime =
            DateTime.UtcNow;

        var ttl =
            record.ExpireTime - DateTime.UtcNow;

        if (ttl <= TimeSpan.Zero)
        {
            ttl = TimeSpan.FromSeconds(1);
        }

        // CAS 更新
        var tran = database.CreateTransaction();

        tran.AddCondition(
            Condition.StringEqual(
                key,
                value));

        _ = tran.StringSetAsync(
            key,
            JsonSerializer.Serialize(record),
            ttl);

        await tran.ExecuteAsync();
    }

    public async Task RemoveAsync(string typeCode, string keyHash, string requestId, CancellationToken cancellationToken = default)
    {
        var key = IdempotencyKeyHelper.BuildStorageKey(
            options.Value.KeyPrefix,
            typeCode,
            keyHash);

        var value = await database.StringGetAsync(key);

        if (!value.HasValue)
        {
            return;
        }

        var record =
            JsonSerializer.Deserialize<RedisIdempotencyRecord>(
                value.ToString());

        if (record == null)
        {
            return;
        }

        // requestId 不一致
        // 不允许删除别人的锁

        if (!string.Equals(
                record.RequestId,
                requestId,
                StringComparison.Ordinal))
        {
            return;
        }

        // CAS 删除

        var tran = database.CreateTransaction();

        tran.AddCondition(
            Condition.StringEqual(
                key,
                value));

        _ = tran.KeyDeleteAsync(key);

        await tran.ExecuteAsync();
    }

    public Task<int> DeleteExpiredAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public Task<int> DeleteCompletedAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }
}
