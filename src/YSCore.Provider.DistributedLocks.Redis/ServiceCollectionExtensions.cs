using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using YSCore.Base.DistributedLocks;
using YSCore.Base.Idempotency;
using Microsoft.Extensions.Options;
using System.Reflection;

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

        public static IServiceCollection AddRedisIdempotencyStore(this IServiceCollection services)
        {
            services.AddSingleton<IIdempotencyStore>(p =>
                new RedisIdempotencyStore(
                    p.GetRequiredService<IConnectionMultiplexer>().GetDatabase(),
                    p.GetRequiredService<IOptions<IdempotencyOptions>>()));
            return services;
        }

        public static IServiceCollection AddRedisIdempotencyStore(
            this IServiceCollection services,
            IConnectionMultiplexer connectionMultiplexer)
        {
            services.AddSingleton<IIdempotencyStore>(p =>
                new RedisIdempotencyStore(
                    connectionMultiplexer.GetDatabase(),
                    p.GetRequiredService<IOptions<IdempotencyOptions>>()));
            return services;
        }

        public static IServiceCollection AddRedisIdempotencyStore(this IServiceCollection services, IDatabase database)
        {
            services.AddSingleton<IIdempotencyStore>(p =>
                new RedisIdempotencyStore(
                    database,
                    p.GetRequiredService<IOptions<IdempotencyOptions>>()));
            return services;
        }

        public static IServiceCollection AddRedisIdempotency(
            this IServiceCollection services,
            Assembly[] assemblies,
            Action<IdempotencyOptions>? configure = null)
        {
            services.AddIdempotency(configure);
            services.AddIdempotentCommands(assemblies);
            services.AddRedisIdempotencyStore();
            return services;
        }
    }
}
