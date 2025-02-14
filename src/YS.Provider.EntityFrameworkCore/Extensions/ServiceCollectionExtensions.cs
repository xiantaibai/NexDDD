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
using ShardingCore.Sharding;

namespace YSCore.Provider.EntityFrameworkCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Scan assemblies add all IRepository to ServiceCollection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services, params Assembly[] assemblies)
        {
            var types = assemblies.SelectMany(assembly => assembly.GetTypes());

            foreach (var repositoryType in types.Where(x =>
                         !x.IsGenericType && !x.IsAbstract && x.BaseType != null && x.BaseType.IsGenericType &&
                         (x.BaseType.GetGenericTypeDefinition() == typeof(YsRepositoryBase<,,>)
                          || x.BaseType.GetGenericTypeDefinition() == typeof(YsRepositoryBase<,>))))
            {
                services.TryAddScoped(repositoryType);
                var repositoryInterfaceType = Array.Find(repositoryType.GetInterfaces(),
                    x => !x.IsGenericType && Array.Exists(x.GetInterfaces(),
                        i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IYsRepository<>)));

                if (repositoryInterfaceType == null)
                    continue;
                services.TryAddScoped(repositoryInterfaceType, p => p.GetRequiredService(repositoryType));
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
        /// 添加工作单元指令
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static MediatRServiceConfiguration AddUnitOfWorkBehaviors(this MediatRServiceConfiguration cfg)
        {
            cfg.AddOpenBehavior(typeof(CommandUnitOfWorkBehavior<,>));
            return cfg;
        }
    }
}
