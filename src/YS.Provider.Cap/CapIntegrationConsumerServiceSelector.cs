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
using YSCore.Base.Extension;
using YSCore.Base.Integration;

namespace YS.Provider.Cap
{
    /// <summary>
    /// Cap 消息订阅封装
    /// </summary>
    public class CapIntegrationConsumerServiceSelector: ConsumerServiceSelector
    {
        private readonly CapOptions _capOptions;

        public CapIntegrationConsumerServiceSelector(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _capOptions = serviceProvider.GetRequiredService<IOptions<CapOptions>>().Value;
        }

        protected override IEnumerable<ConsumerExecutorDescriptor> FindConsumersFromInterfaceTypes(IServiceProvider provider)
        {
            var executorDescriptorList = new List<ConsumerExecutorDescriptor>();

            using (var scoped = provider.CreateScope())
            {
                var hanletype = typeof(IIntegrationEventHandler<>);
                var hanletypeTypeInfo = hanletype.GetTypeInfo();

                var handlerServices = scoped.ServiceProvider.GetServices(hanletype);

                foreach (var handler in handlerServices)
                {
                    var typeInfo = handler.GetType().GetTypeInfo();
                    if (typeInfo == null || !hanletypeTypeInfo.IsAssignableFrom(typeInfo))
                    {
                        continue;
                    }
                    executorDescriptorList.AddRange(GetCapDescription(typeInfo));
                }
            }
            return executorDescriptorList;
        }

        private IEnumerable<ConsumerExecutorDescriptor> GetCapDescription(TypeInfo typeInfo)
        {
            var method = typeInfo.DeclaredMethods.FirstOrDefault(x => x.Name == "Handle");
            if (method == null) yield break;

            var topic = _capOptions.DefaultGroupName + "." + _capOptions.Version;
            var capName = typeInfo.GetGenericTypeName();

            var parameters = method.GetParameters()
                   .Select(parameter => new ParameterDescriptor
                   {
                       Name = parameter.Name!,
                       ParameterType = parameter.ParameterType,
                       IsFromCap = parameter.GetCustomAttributes(typeof(FromCapAttribute)).Any()
                                   || typeof(CancellationToken).IsAssignableFrom(parameter.ParameterType)
                   }).ToList();

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
    }
}
