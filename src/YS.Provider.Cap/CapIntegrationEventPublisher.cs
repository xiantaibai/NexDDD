using DotNetCore.CAP;
using YSCore.Base.Extensions;
using YSCore.Base.Integration;

namespace YS.Provider.Cap
{
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
