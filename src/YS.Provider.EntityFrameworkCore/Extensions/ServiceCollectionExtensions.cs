using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.TableExists.Abstractions;
using ShardingCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.DatabaseAccessor;
using YSCore.Base.DatabaseAccessor.Repositories;
using YSCore.Provider.EntityFrameworkCore.Behaviors;
using YSCore.Provider.EntityFrameworkCore.Repository;
using ShardingCore.Sharding;

namespace YSCore.Provider.EntityFrameworkCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 扫描程序集并注册写仓储。
        /// 保留此方法用于兼容旧用法，内部委托给 AddWriteRepositories。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services, params Assembly[] assemblies)
        {
            return services.AddWriteRepositories(assemblies);
        }

        /// <summary>
        /// 扫描程序集并注册所有写仓储。
        /// 写仓储必须基于聚合根。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddWriteRepositories(this IServiceCollection services, params Assembly[] assemblies)
        {
            var types = assemblies.SelectMany(assembly => assembly.GetTypes());

            foreach (var repositoryType in types.Where(x =>
                         !x.IsGenericType && !x.IsAbstract
                         && (IsSubclassOfRawGeneric(typeof(YsRepositoryBase<,,>), x)
                             || IsSubclassOfRawGeneric(typeof(YsRepositoryBase<,>), x))))
            {
                services.TryAddScoped(repositoryType);
                var repositoryInterfaceTypes = repositoryType.GetInterfaces()
                    .Where(x => !x.IsGenericType && Array.Exists(x.GetInterfaces(),
                        i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IYsRepository<>)))
                    .ToArray();

                foreach (var repositoryInterfaceType in repositoryInterfaceTypes)
                {
                    services.TryAddScoped(repositoryInterfaceType, p => p.GetRequiredService(repositoryType));
                }
            }

            return services;
        }

        /// <summary>
        /// 为指定程序集中的每个实体注册默认 IQueryRepository/IReadableRepository。
        /// 普通查询场景优先使用此通用查询仓储；只有通用仓储不满足时，再定义自定义查询仓储。
        /// </summary>
        /// <typeparam name="TDbContext">读 DbContext，通常绑定到只读库或从库。</typeparam>
        /// <param name="services"></param>
        /// <param name="entityAssemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddQueryRepositories<TDbContext>(this IServiceCollection services, params Assembly[] entityAssemblies)
            where TDbContext : DbContext
        {
            var entityTypes = entityAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(x => !x.IsAbstract && !x.IsGenericType && typeof(Entity).IsAssignableFrom(x))
                .ToArray();

            foreach (var entityType in entityTypes)
            {
                RegisterDefaultQueryRepository<TDbContext>(services, entityType);
            }

            return services;
        }

        /// <summary>
        /// 注册默认通用查询仓储，并扫描注册自定义只读/查询仓储。
        /// </summary>
        /// <typeparam name="TDbContext">读 DbContext，通常绑定到只读库或从库。</typeparam>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddReadableRepositories<TDbContext>(this IServiceCollection services, params Assembly[] assemblies)
            where TDbContext : DbContext
        {
            services.AddQueryRepositories<TDbContext>(assemblies);
            services.AddReadableRepositories(assemblies);
            return services;
        }

        /// <summary>
        /// 扫描程序集并注册自定义只读/查询仓储。
        /// 查询仓储可以面向任意实体，也可以使用只读 DbContext、原生 SQL 或自定义查询实现。
        /// 此处会刻意排除写仓储，避免读模型被意外路由到写库。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddReadableRepositories(this IServiceCollection services, params Assembly[] assemblies)
        {
            var types = assemblies.SelectMany(assembly => assembly.GetTypes());

            foreach (var repositoryType in types.Where(x =>
                         !x.IsGenericType && !x.IsAbstract
                         && ImplementsGenericInterface(x, typeof(IReadableRepository<>))
                         && !ImplementsGenericInterface(x, typeof(IYsRepository<>))))
            {
                services.TryAddScoped(repositoryType);

                var repositoryInterfaceTypes = repositoryType.GetInterfaces()
                    .Where(x => !x.IsGenericType && ImplementsGenericInterface(x, typeof(IReadableRepository<>)))
                    .ToArray();

                foreach (var repositoryInterfaceType in repositoryInterfaceTypes)
                {
                    services.TryAddScoped(repositoryInterfaceType, p => p.GetRequiredService(repositoryType));
                }
            }

            return services;
        }

        public static IServiceCollection AddUnitOfWork<TDbContext>(this IServiceCollection services)
           where TDbContext : IUnitOfWork, ITransactionUnitOfWork
        {
            services.AddScoped<IUnitOfWork>(p => p.GetRequiredService<TDbContext>());
            services.AddScoped<ITransactionUnitOfWork>(p => p.GetRequiredService<TDbContext>());
            return services;
        }

        /// <summary>
        /// 添加工作单元指令。
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static MediatRServiceConfiguration AddUnitOfWorkBehaviors(this MediatRServiceConfiguration cfg)
        {
            cfg.AddOpenBehavior(typeof(CommandUnitOfWorkBehavior<,>));
            return cfg;
        }

        private static void RegisterDefaultQueryRepository<TDbContext>(IServiceCollection services, Type entityType)
            where TDbContext : DbContext
        {
            var queryRepositoryType = typeof(IQueryRepository<>).MakeGenericType(entityType);
            var readableRepositoryType = typeof(IReadableRepository<>).MakeGenericType(entityType);
            var implementationType = typeof(ReadableRepository<,>).MakeGenericType(entityType, typeof(TDbContext));

            services.TryAddScoped(queryRepositoryType, implementationType);
            services.TryAdd(ServiceDescriptor.Scoped(readableRepositoryType, provider => provider.GetRequiredService(queryRepositoryType)));
        }

        private static bool ImplementsGenericInterface(Type type, Type genericInterfaceType)
        {
            return type.GetInterfaces()
                .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericInterfaceType);
        }

        private static bool IsSubclassOfRawGeneric(Type genericType, Type type)
        {
            while (type != null && type != typeof(object))
            {
                var currentType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (genericType == currentType)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
