using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YSCore.Base.Idempotency;

namespace YSCore.Provider.EntityFrameworkCore.Idempotency;

/// <summary>
/// 幂等初始化扩展。
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 初始化幂等表。
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文类型</typeparam>
    /// <param name="serviceProvider">服务提供器</param>
    /// <returns>服务提供器</returns>
    public static async Task<IServiceProvider> InitializeIdempotencyTableAsync<TDbContext>(this IServiceProvider serviceProvider)
        where TDbContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var sql = IdempotencyTableInitializer.GetCreateTableSql(dbContext.Database.ProviderName);
        await dbContext.Database.ExecuteSqlRawAsync(sql);
        return serviceProvider;
    }

    /// <summary>
    /// 添加数据库版幂等全套服务。
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="assemblies">命令幂等配置所在程序集</param>
    /// <param name="configure">幂等配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDbIdempotency<TDbContext>(
        this IServiceCollection services,
        Assembly[] assemblies,
        Action<IdempotencyOptions>? configure = null)
        where TDbContext : DbContext
    {
        services.AddIdempotency(configure);
        services.AddIdempotentCommands(assemblies);
        services.AddDbIdempotencyStore<TDbContext>();
        return services;
    }
}
