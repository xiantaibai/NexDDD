using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YSCore.Provider.EntityFrameworkCore;

namespace YS.Provider.Cap.Mysql
{
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
