using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.DependencyInjection.Dependencies;

namespace NexDDD.Core.DependencyInjection.Extensions
{
    /// <summary>
    /// 依赖注入扩展类
    /// 负责依赖注入模块中的 DependencyInjectionServiceCollectionExtensions 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    public static class DependencyInjectionServiceCollectionExtensions
    {
        public static IServiceCollection AddInnerDependencyInjection(this IServiceCollection services, params Assembly[] assemblies)
        {
            // 查找所有需要注入的类型
            var injectTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(u => typeof(IPrivateDependency).IsAssignableFrom(u) && u.IsClass && !u.IsInterface && !u.IsAbstract);
            // .OrderBy()

            var lifetimeInterfaces = new[] { typeof(ITransient), typeof(IScoped), typeof(ISingleton) };
            foreach (var type in injectTypes)
            {
                var interfaces = type.GetInterfaces();

                // 获取所有能注册的接口
                var canInjectInterfaces = interfaces.Where(u => u != typeof(IDisposable)
                                && u != typeof(IAsyncDisposable)
                                && u != typeof(IPrivateDependency)
                                && !lifetimeInterfaces.Contains(u)
                                && (
                                    (!type.IsGenericType && !u.IsGenericType)
                                    || (type.IsGenericType && u.IsGenericType && type.GetGenericArguments().Length == u.GetGenericArguments().Length))
                                );

                // 获取生存周期类型
                var dependencyType = interfaces.Last(u => lifetimeInterfaces.Contains(u));

                RegisterService(services, dependencyType, type, canInjectInterfaces);
            }

            return services;
        }

        private static void RegisterService(IServiceCollection services, Type dependencyType, Type type,  IEnumerable<Type> canInjectInterfaces)
        {
            if (!canInjectInterfaces.Any()) return;

            var lifetime = TryGetServiceLifetime(dependencyType);

            foreach (var inter in canInjectInterfaces)
            {
                if (inter == null) services.TryAdd(ServiceDescriptor.Describe(type, type, lifetime));
                else services.Add(ServiceDescriptor.Describe(inter, type, lifetime));
            }
        }

        /// <summary>
        /// 根据依赖接口类型解析 ServiceLifetime 对象
        /// </summary>
        /// <param name="dependencyType"></param>
        /// <returns></returns>
        private static ServiceLifetime TryGetServiceLifetime(Type dependencyType)
        {
            if (dependencyType == typeof(ITransient))
            {
                return ServiceLifetime.Transient;
            }
            else if (dependencyType == typeof(IScoped))
            {
                return ServiceLifetime.Scoped;
            }
            else if (dependencyType == typeof(ISingleton))
            {
                return ServiceLifetime.Singleton;
            }
            else
            {
                throw new InvalidCastException("Invalid service registration lifetime.");
            }
        }
    }
}
