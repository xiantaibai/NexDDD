using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Provider.EntityFrameworkCore;

namespace NexDDD.Provider.Cap.Mysql
{
    /// <summary>
    /// 提供CAP 集成事件模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Mysql事务服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMySqlTransactionHandler(this IServiceCollection services)
        {
            services.TryAddScoped<IPublisherTransactionHandler, CapMySqlPublisherTransactionHandler>();
            return services;
        }
    }
}
