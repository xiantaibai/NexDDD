using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.Extension;

namespace YSCore.Base.Integration
{
    public static class IntegrationExtensions
    {
        /// <summary>
        /// 存储注册的EventHandler类型
        /// </summary>
        /// <param name="services"></param>
        /// <param name="typeFromAssemblies"></param>
        /// <returns></returns>
        public static IIntegrationEventServicesBuilder AddIntegrationEventServices(this IServiceCollection services,
            params Type[] typeFromAssemblies)
        {
            var types = typeFromAssemblies.Distinct().Select(p => p.Assembly).SelectMany(assembly => assembly.GetTypes());
            var handlers = types.Where(t =>
               t is { IsClass: true, IsAbstract: false } && Array.Exists(t.GetInterfaces(), p =>
                   p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)));

            foreach (var handler in handlers)
            {
                var interfaceType = handler.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));
                services.TryAddEnumerable(new ServiceDescriptor(interfaceType, handler, ServiceLifetime.Scoped));
                // key 值对象，IIntegrationEvent 
                services.Configure<IntergrationEventSubscriptionInfo>(o =>
                {
                    o.EventTypes[interfaceType.GetGenericArguments().First().GetGenericTypeName()] = handler;
                });
            }

            return new IntegrationEventServicesBuilder(services);
        }

        private class IntegrationEventServicesBuilder(IServiceCollection services) : IIntegrationEventServicesBuilder
        {
            public IServiceCollection Services => services;
        }
    }
}
