using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using YSCore.Base.Idempotency;

namespace YSCore.Provider.DistributedLocks.Redis;

/// <summary>
/// 基于 Redis 的幂等存储。
/// </summary>
public sealed class RedisIdempotencyStore(
    IDatabase database,
    IOptions<IdempotencyOptions> options) : IIdempotencyStore
{
    public async Task<bool> TryBeginAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
    {
        var key = IdempotencyKeyHelper.BuildStorageKey(options.Value.KeyPrefix, record.TypeCode, record.KeyHash);
        var ttl = record.ExpireTime - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            ttl = TimeSpan.FromSeconds(1);
        }

        var payload = JsonSerializer.Serialize(record);
        return await database.StringSetAsync(key, payload, ttl, When.NotExists);
    }

    public async Task MarkCompletedAsync(string typeCode, string keyHash, CancellationToken cancellationToken = default)
    {
        var key = IdempotencyKeyHelper.BuildStorageKey(options.Value.KeyPrefix, typeCode, keyHash);
        var value = await database.StringGetAsync(key);
        if (!value.HasValue)
        {
            return;
        }

        var record = JsonSerializer.Deserialize<IdempotencyRecord>(value!)
                     ?? new IdempotencyRecord
                     {
                         TypeCode = typeCode,
                         KeyHash = keyHash,
                         CreateTime = DateTime.UtcNow,
                         ExpireTime = DateTime.UtcNow.Add(options.Value.DefaultExpire)
                     };

        record.Status = IdempotencyStatus.Completed;
        var ttl = await database.KeyTimeToLiveAsync(key);
        await database.StringSetAsync(key, JsonSerializer.Serialize(record), ttl);
    }

    public async Task RemoveAsync(string typeCode, string keyHash, CancellationToken cancellationToken = default)
    {
        var key = IdempotencyKeyHelper.BuildStorageKey(options.Value.KeyPrefix, typeCode, keyHash);
        await database.KeyDeleteAsync(key);
    }

    public Task<int> DeleteExpiredAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }
}
