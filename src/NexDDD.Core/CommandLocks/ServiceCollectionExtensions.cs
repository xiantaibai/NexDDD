using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using NexDDD.Core.DistributedLocks;

namespace NexDDD.Core.CommandLocks
{
    /// <summary>
    /// 提供命令并发控制模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加CommandLockBehavior，以支持命令锁
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static MediatRServiceConfiguration AddCommandLockBehavior(this MediatRServiceConfiguration cfg)
        {
            cfg.AddOpenBehavior(typeof(CommandLockBehavior<,>));
            return cfg;
        }

        /// <summary>
        /// 添加CommandLocks
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddCommandLocks(this IServiceCollection services, params Assembly[] assemblies)
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
                            @interface.GetGenericTypeDefinition() == typeof(ICommandLock<>))
                        {
                            services.AddTransient(@interface, type);
                        }
                    }
                }
            }

            return services;
        }
    }
}