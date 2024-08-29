using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.Integration;

namespace YS.Provider.Cap
{
    public static class CapServiceExtensions
    {
        public static IIntegrationEventServicesBuilder UseCap(this IIntegrationEventServicesBuilder builder)
        {
            builder.Services.AddSingleton<IConsumerServiceSelector, CapIntegrationConsumerServiceSelector>();

            builder.Services.AddSingleton<IIntegrationEventPublisher, CapIntegrationEventPublisher>();

            return builder;
        }
    }
}
