using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Provider.EntityFrameworkCore;

namespace YSCore.Provider.Cap.Mysql
{
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
