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
