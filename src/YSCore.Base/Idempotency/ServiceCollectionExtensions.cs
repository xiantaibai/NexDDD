using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace YSCore.Base.Idempotency;

/// <summary>
/// 幂等服务注册扩展。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加幂等行为。
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    public static MediatRServiceConfiguration AddIdempotentBehavior(this MediatRServiceConfiguration cfg)
    {
        cfg.AddOpenBehavior(typeof(IdempotentBehavior<,>));
        return cfg;
    }

    /// <summary>
    /// 添加幂等基础服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddIdempotency(
        this IServiceCollection services,
        Action<IdempotencyOptions>? configure = null)
    {
        var optionsBuilder = services.AddOptions<IdempotencyOptions>();
        if (configure != null)
        {
            optionsBuilder.Configure(configure);
        }

        services.AddHostedService<IdempotencyCleanupHostedService>();
        return services;
    }

    /// <summary>
    /// 扫描并注册所有命令幂等配置类型。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddIdempotentCommands(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();

            foreach (var type in types.Where(p => p is { IsClass: true, IsAbstract: false, IsGenericType: false }))
            {
                var interfaces = type.GetInterfaces();
                foreach (var @interface in interfaces)
                {
                    if (@interface.IsGenericType &&
                        @interface.GetGenericTypeDefinition() == typeof(IIdempotentCommand<>))
                    {
                        services.AddTransient(@interface, type);
                    }
                }
            }
        }

        return services;
    }
}
