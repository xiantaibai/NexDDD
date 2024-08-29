using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Integration
{
    /// <summary>
    /// 集成事件发布
    /// </summary>
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default) where TIntegrationEvent : IIntegrationEvent;
    }
}
