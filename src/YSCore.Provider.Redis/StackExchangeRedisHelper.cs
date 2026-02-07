using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Protobuf;
using StackExchange.Redis.Extensions.System.Text.Json;
using System.Collections.Concurrent;
using System.Text.Json;

namespace YSCore.Provider.Redis
{
    /// <summary>
    /// StackExchange redis 帮助类
    /// </summary>
    public class StackExchangeRedisHelper
    {
        private readonly IRedisDatabase _redis;
        private readonly IRedisDatabase _lockRedis; // 锁单独数据库（也可用同一个）

        public StackExchangeRedisHelper(IRedisDatabase redis)
        {
            _redis = redis;
            _lockRedis = redis;
        }

        /// <summary>
        /// StackExchange.Redis.Extensions.Core IRedisDatabase
        /// </summary>
        /// <returns></returns>
        public IRedisDatabase GetRedisDatabase()
        {
            return _redis;
        }

        #region String

        /// <summary>
        /// 获取Key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T?> GetAsync<T>(string key)
        {
            return await _redis.GetAsync<T>(key);
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (expiry.HasValue)
                return await _redis.AddAsync(key, value, expiry.Value);
            else
                return await _redis.AddAsync(key, value);
        }

        /// <summary>
        /// 获取 Redis 值，如果不存在则调用 factory 获取并写入 Redis
        /// </summary>
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            var value = await _redis.GetAsync<T>(key);
            if (value != null)
                return value;

            // 调用委托获取数据
            var result = await factory();

            if (result != null)
            {
                if (expiry.HasValue)
                    await _redis.AddAsync(key, result, expiry.Value);
                else
                    await _redis.AddAsync(key, result);
            }

            return result;
        }

        /// <summary>
        /// 删除Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<bool> RemoveAsync(string key)
        {
            return _redis.RemoveAsync(key);
        }

        #endregion

        #region 递增递减 
        /// <summary>
        /// 自增 1（原子）
        /// </summary>
        public async Task<long> IncrementAsync(string key)
        {
            return await _redis.Database.StringIncrementAsync(key);
        }

        /// <summary>
        /// 自减 1（原子）
        /// </summary>
        public async Task<long> DecrementAsync(string key)
        {
            return await _redis.Database.StringDecrementAsync(key);
        }

        /// <summary>
        /// 指定增量递增
        /// </summary>
        public async Task<long> IncrementByAsync(string key, long value)
        {
            return await _redis.Database.StringIncrementAsync(key, value);
        }

        /// <summary>
        /// 指定减量递减
        /// </summary>
        public async Task<long> DecrementByAsync(string key, long value)
        {
            return await _redis.Database.StringDecrementAsync(key, value);
        }

        /// <summary>
        /// 严格递减, 当值不能扣减时返回 -1
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="delta">扣减数量,不能扣减返回-1</param>
        /// <returns></returns>
        public async Task<long> TryDecrementAsync(string key, long delta = 1)
        {
            var db = _redis.Database;

            const string script = @"
            local v = redis.call('GET', KEYS[1])
            if not v then
                return -1
            end

            v = tonumber(v)
            local d = tonumber(ARGV[1])

            if v < d then
                return -1
            end

            return redis.call('DECRBY', KEYS[1], d)
            ";

            return (long)await db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { key },
                new RedisValue[] { delta });
        }

        /// <summary>
        /// 如果不存在则初始化并设置过期时间，再递增
        /// 适合统计型 Key
        /// </summary>
        public async Task<long> IncrementWithExpireAsync(
            string key,
            long delta,
            TimeSpan expiry)
        {
            var db = _redis.Database;

            // Lua 保证原子
            const string script = @"
local v = redis.call('INCRBY', KEYS[1], ARGV[1])
if v == tonumber(ARGV[1]) then
    redis.call('EXPIRE', KEYS[1], ARGV[2])
end
return v";

            return (long)await db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { key },
                new RedisValue[]
                {
            delta,
            (long)expiry.TotalSeconds
                });
        }

        /// <summary>
        /// 获取当前计数
        /// </summary>
        public async Task<long?> GetCounterAsync(string key)
        {
            var value = await _redis.Database.StringGetAsync(key);
            if (!value.HasValue)
                return null;

            return (long)value;
        }
        #endregion

        #region 分布式锁

        /// <summary>
        /// 创建分布式锁对象
        /// </summary>
        public RedisLock CreateLock(string key, TimeSpan expiry)
        {
            var db = _lockRedis.Database;
            return new RedisLock(db, key, expiry);
        }

        #endregion
    }
}
