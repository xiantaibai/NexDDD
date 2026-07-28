using DotNetCore.CAP;
using NexDDD.Core.Extensions;
using NexDDD.Core.Integration;

namespace NexDDD.Provider.Cap
{
    /// <summary>
    /// 负责集成事件模块中的 CapIntegrationEventPublisher 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public class CapIntegrationEventPublisher : IIntegrationEventPublisher
    {
        private readonly ICapPublisher _capPublisher;

        public CapIntegrationEventPublisher(ICapPublisher capPublisher)
        {
            _capPublisher = capPublisher;
        }

        public Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default) where TIntegrationEvent : IIntegrationEvent
        {
            return _capPublisher.PublishAsync(name: integrationEvent.GetGenericTypeName(),
                contentObj: integrationEvent,
                cancellationToken: cancellationToken);
        }

        public Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, IDictionary<string, string?> headers, CancellationToken cancellationToken = default) where TIntegrationEvent : IIntegrationEvent
        {
            return _capPublisher.PublishAsync(name: integrationEvent.GetGenericTypeName(),
                contentObj: integrationEvent,
                headers: headers,
                cancellationToken: cancellationToken);
        }
    }
}
