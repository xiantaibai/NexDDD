using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Integration
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> where TIntegrationEvent: IIntegrationEvent
    {
        public Task HandleAsync(TIntegrationEvent eventData, CancellationToken cancellationToken = default);
    }
}
