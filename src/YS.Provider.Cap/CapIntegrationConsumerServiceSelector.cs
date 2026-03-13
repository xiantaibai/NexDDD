using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.Extensions;
using YSCore.Base.Integration;

namespace YSCore.Provider.Cap
{
    /// <summary>
    /// Cap 消息订阅封装
    /// </summary>
    public class CapIntegrationConsumerServiceSelector : ConsumerServiceSelector
    {
        private readonly CapOptions _capOptions;
        private readonly IntergrationEventSubscriptionInfo _subscriptionInfo;

        public CapIntegrationConsumerServiceSelector(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _capOptions = serviceProvider.GetRequiredService<IOptions<CapOptions>>().Value;
            _subscriptionInfo = serviceProvider.GetRequiredService<IOptions<IntergrationEventSubscriptionInfo>>().Value;
        }

        protected override IEnumerable<ConsumerExecutorDescriptor> FindConsumersFromInterfaceTypes(IServiceProvider provider)
        {
            var executorDescriptorList = new List<ConsumerExecutorDescriptor>();

            using (var scoped = provider.CreateScope())
            {
                foreach (var key in _subscriptionInfo.EventTypes)
                {
                    var typeInfo = key.Value.GetTypeInfo();
                    if (typeInfo == null)
                    {
                        continue;
                    }
                    executorDescriptorList.AddRange(GetCapDescription(typeInfo, key.Key));
                }

            }
            return executorDescriptorList;
        }

        private IEnumerable<ConsumerExecutorDescriptor> GetCapDescription(TypeInfo typeInfo, string capName)
        {
            var method = typeInfo.DeclaredMethods.FirstOrDefault(x => x.Name == "HandleAsync" &&
            x.GetParameters().Any(p => p.ParameterType == typeof(IntegrationEventHandlerContext)));
            if (method == null) yield break;

            var topic = _capOptions.DefaultGroupName + "." + _capOptions.Version;
            // var capName = typeInfo.GetGenericTypeName();
            var eventType = GetEventType(typeInfo);

            var parameters = new List<ParameterDescriptor>();
            foreach (var parameter in method.GetParameters())
            {
                if (parameter.ParameterType == typeof(IntegrationEventHandlerContext))
                {
                    parameters.Add(new ParameterDescriptor
                    {
                        Name = parameter.Name!,
                        ParameterType = eventType, // ★关键
                        IsFromCap = false
                    });
                }
                else
                {
                    parameters.Add(new ParameterDescriptor
                    {
                        Name = parameter.Name!,
                        ParameterType = parameter.ParameterType,
                        IsFromCap =
                            parameter.GetCustomAttributes(typeof(FromCapAttribute)).Any()
                            || typeof(CancellationToken).IsAssignableFrom(parameter.ParameterType)
                    });
                }
            }

            yield return new ConsumerExecutorDescriptor
            {
                Attribute = new CapSubscribeAttribute(capName)
                {
                    Group = topic
                },
                Parameters = parameters,
                MethodInfo = method,
                ImplTypeInfo = typeInfo,
                TopicNamePrefix = _capOptions.TopicNamePrefix,
                ServiceTypeInfo = typeInfo
            };
        }

        private static Type GetEventType(Type handlerType)
        {
            return handlerType
                .GetInterfaces()
                .First(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
                .GetGenericArguments()[0];
        }
    }
}
