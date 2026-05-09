using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using YSCore.Base.Idempotency;

namespace YSCore.Provider.EntityFrameworkCore.Idempotency;

/// <summary>
/// 数据库幂等存储注册扩展。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加数据库版幂等存储。
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDbIdempotencyStore<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IIdempotencyStore, DbIdempotencyStore<TDbContext>>();
        return services;
    }
}
