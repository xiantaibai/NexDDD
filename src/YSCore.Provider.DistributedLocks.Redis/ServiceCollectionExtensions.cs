using Microsoft.Extensions.DependencyInjection;
using YSCore.Base.DistributedLocks;
using StackExchange.Redis;

namespace YSCore.Provider.DistributedLocks.Redis
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisLocks(this IServiceCollection services)
        {
            services.AddSingleton<IDistributedLock>(p =>
                new RedisLock(p.GetRequiredService<IConnectionMultiplexer>().GetDatabase()));
            return services;
        }

        public static IServiceCollection AddRedisLocks(this IServiceCollection services,
            IConnectionMultiplexer connectionMultiplexer)
        {
            services.AddSingleton<IDistributedLock>(p =>
                new RedisLock(connectionMultiplexer.GetDatabase()));
            return services;
        }

        public static IServiceCollection AddRedisLocks(this IServiceCollection services, IDatabase database)
        {
            services.AddSingleton<IDistributedLock>(p =>
                new RedisLock(database));
            return services;
        }
    }
}