using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.Integration;

namespace NexDDD.Provider.Cap
{
    /// <summary>
    /// 提供CAP 集成事件模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
    public static class CapServiceExtensions
    {
        public static IIntegrationEventServicesBuilder UseCap(this IIntegrationEventServicesBuilder builder)
        {
            builder.Services.AddSingleton<IConsumerServiceSelector, CapIntegrationConsumerServiceSelector>();

            builder.Services.AddSingleton<ISubscribeInvoker, CapIIntegrationSubscribeInvoker>();

            builder.Services.AddSingleton<IIntegrationEventPublisher, CapIntegrationEventPublisher>();

            return builder;
        }
    }
}
