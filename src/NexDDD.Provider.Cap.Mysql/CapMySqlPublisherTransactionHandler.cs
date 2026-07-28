using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexDDD.Provider.EntityFrameworkCore;

namespace NexDDD.Provider.Cap.Mysql
{
    /// <summary>
    /// 负责CAP 集成事件流程的处理与编排，协调上游请求和下游服务完成当前用例。
    /// </summary>
    public class CapMySqlPublisherTransactionHandler : IPublisherTransactionHandler
    {
        private readonly Lazy<ICapPublisher> _capBus; //lazy load to avoid circular dependency
        public CapMySqlPublisherTransactionHandler(IServiceProvider serviceProvider)
        {
            _capBus = new Lazy<ICapPublisher>(() => serviceProvider.GetRequiredService<ICapPublisher>());
        }

        public IDbContextTransaction BeginTransaction(DbContext context)
        {
            return context.Database.BeginTransaction(_capBus.Value, autoCommit: false);
        }
    }
}
