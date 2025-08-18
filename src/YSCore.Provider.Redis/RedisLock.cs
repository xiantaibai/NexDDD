using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Provider.Redis
{
    /// <summary>
    /// redis 锁封装
    /// </summary>
    public class RedisLock : IAsyncDisposable
    {
        private readonly IDatabase _db;
        private readonly string _lockKey;
        private readonly string _lockValue;
        private readonly TimeSpan _expiry;

        public bool Acquired { get; private set; }

        public RedisLock(IDatabase db, string key, TimeSpan expiry)
        {
            _db = db;
            _lockKey = key;
            _lockValue = Guid.NewGuid().ToString("N");
            _expiry = expiry;
        }

        public async Task<bool> AcquireAsync(int retryCount = 3, int retryDelayMs = 100)
        {
            var rand = new Random();
            for (int i = 0; i < retryCount; i++)
            {
                Acquired = await _db.StringSetAsync(_lockKey, _lockValue, _expiry, When.NotExists);
                if (Acquired) return true;

                // 指数退避 + 随机抖动
                var delay = retryDelayMs * (int)Math.Pow(2, i) + rand.Next(0, 50);
                await Task.Delay(delay);
            }
            return false;
        }

        public async ValueTask DisposeAsync()
        {
            if (!Acquired) return;

            const string lua = @"
            if redis.call('GET', KEYS[1]) == ARGV[1] then
                return redis.call('DEL', KEYS[1])
            else
                return 0
            end";

            await _db.ScriptEvaluateAsync(lua, new RedisKey[] { _lockKey }, new RedisValue[] { _lockValue });
        }
    }
}
