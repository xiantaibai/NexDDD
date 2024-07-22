using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YSCore.Base.DatabaseAccessor;
using YSCore.Base.Diagnostics;

namespace YS.Provider.EntityFrameworkCore.Extensions
{
    public static class MediatorExtension
    {
        private static readonly DiagnosticListener _diagnosticListener =
            new DiagnosticListener(YsNetCoreDiagnosticListenerNames.DiagnosticListenerName);

        public static async Task DispatchDomainEventsAsync(this IMediator mediator, AppDbContextBase ctx, CancellationToken cancellationToken = default)
        {
            var domainEntities = ctx.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.GetDomainEvents().Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.GetDomainEvents())
                .ToList();

            if (domainEvents.Count == 0)
            {
                return;
            }

            domainEntities.ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            //foreach (var domainEvent in domainEvents)
            //    await mediator.Publish(domainEvent);
            foreach (var domainEvent in domainEvents)
            {
                Guid id = Guid.NewGuid();
                var eventType = domainEvent.GetType();
                var eventTypeName = eventType.FullName ?? eventType.Name;
                try
                {
                    WriteDomainEventHandlerBegin(new DomainEventHandlerBegin(id, eventTypeName, domainEvent));
                    await mediator.Publish(domainEvent, cancellationToken);
                    WriteDomainEventHandlerEnd(new DomainEventHandlerEnd(id, eventTypeName, domainEvent));
                }
                catch (Exception e)
                {
                    WriteDomainEventError(new DomainEventHandlerError(id, eventTypeName, domainEvent, e));
                    throw;
                }
            }
        }

        static void WriteDomainEventHandlerBegin(DomainEventHandlerBegin data)
        {
            if (_diagnosticListener.IsEnabled(YsNetCoreDiagnosticListenerNames.DomainEventHandlerBegin))
            {
                _diagnosticListener.Write(YsNetCoreDiagnosticListenerNames.DomainEventHandlerBegin, data);
            }
        }

        static void WriteDomainEventHandlerEnd(DomainEventHandlerEnd data)
        {
            if (_diagnosticListener.IsEnabled(YsNetCoreDiagnosticListenerNames.DomainEventHandlerEnd))
            {
                _diagnosticListener.Write(YsNetCoreDiagnosticListenerNames.DomainEventHandlerEnd, data);
            }
        }

        static void WriteDomainEventError(DomainEventHandlerError data)
        {
            if (_diagnosticListener.IsEnabled(YsNetCoreDiagnosticListenerNames.DomainEventHandlerError))
            {
                _diagnosticListener.Write(YsNetCoreDiagnosticListenerNames.DomainEventHandlerError, data);
            }
        }
    }
}
