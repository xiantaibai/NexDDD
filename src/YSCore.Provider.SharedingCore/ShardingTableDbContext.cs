using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using MediatR;
using YSCore.Base.DatabaseAccessor.Filter;
using YSCore.Provider.SharedingCore.Extensions;
using Microsoft.EntityFrameworkCore.Storage;
using YSCore.Base.DatabaseAccessor;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using YSCore.Base.Diagnostics;

namespace YSCore.Provider.SharedingCore
{
    /// <summary>
    /// 分表上下文
    /// </summary>
    public class ShardingTableDbContext : AbstractShardingDbContext, IShardingTableDbContext, ITransactionUnitOfWork
    {
        private static readonly DiagnosticListener _diagnosticListener =
            new(YsNetCoreDiagnosticListenerNames.DiagnosticListenerName);

        private readonly IMediator _mediator;
        private readonly IPublisherTransactionHandler? _publisherTransactionFactory;
        public ShardingTableDbContext(DbContextOptions options, IMediator mediator, IServiceProvider provider) : base(options)
        {
            _mediator = mediator;
            _publisherTransactionFactory = provider.GetService<IPublisherTransactionHandler>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IsDelete 过滤器
            modelBuilder.AddQueryFilter<IDeletedFilter>(x => x.IsDelete == false);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            configurationBuilder.Properties<string>()
                                .HaveMaxLength(256);
        }

        public IRouteTail RouteTail { get; set; }


        #region IUnitOfWork

        public IDbContextTransaction? CurrentTransaction { get; private set; }

        public IDbContextTransaction BeginTransaction()
        {
            if (_publisherTransactionFactory != null)
            {
                CurrentTransaction = _publisherTransactionFactory.BeginTransaction(this);
            }
            else
            {
                CurrentTransaction = Database.BeginTransaction();
            }
            WriteTransactionBegin(CurrentTransaction.TransactionId);
            return CurrentTransaction;
        }


        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (CurrentTransaction != null)
            {
                await CurrentTransaction.CommitAsync(cancellationToken);
                WriteTransactionCommit(CurrentTransaction.TransactionId);
                CurrentTransaction = null;
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (CurrentTransaction != null)
            {
                await CurrentTransaction.RollbackAsync(cancellationToken);
                WriteTransactionRollback(CurrentTransaction.TransactionId);
                CurrentTransaction = null;
            }
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            if (CurrentTransaction == null)
            {
                CurrentTransaction = this.BeginTransaction();
                await using (CurrentTransaction)
                {
                    try
                    {
                        // ensure field 'Id' initialized when new entity added
                        await BeforeSaveEntitiesAsync(cancellationToken);
                        await base.SaveChangesAsync(cancellationToken);
                        await _mediator.DispatchDomainEventsAsync(this, cancellationToken);
                        await CommitAsync(cancellationToken);
                        return true;
                    }
                    catch
                    {
                        await RollbackAsync(cancellationToken);
                        throw;
                    }
                }
            }
            else
            {
                await BeforeSaveEntitiesAsync(cancellationToken);
                await base.SaveChangesAsync(cancellationToken);
                await _mediator.DispatchDomainEventsAsync(this, cancellationToken);
                return true;
            }
        }

        protected internal virtual Task BeforeSaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            var modifiedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

            foreach (var entry in modifiedEntries)
            {
                if (entry.Entity is BaseEntity myEntity)
                {
                    myEntity.LastModifyTime = DateTime.UtcNow; // 设置为当前时间
                }
            }

            return Task.CompletedTask;
        }

        #endregion

        #region DiagnosticListener

        void WriteTransactionBegin(Guid data)
        {
            if (_diagnosticListener.IsEnabled(YsNetCoreDiagnosticListenerNames.TransactionBegin))
            {
                _diagnosticListener.Write(YsNetCoreDiagnosticListenerNames.TransactionBegin, data);
            }
        }

        void WriteTransactionCommit(Guid data)
        {
            if (_diagnosticListener.IsEnabled(YsNetCoreDiagnosticListenerNames.TransactionCommit))
            {
                _diagnosticListener.Write(YsNetCoreDiagnosticListenerNames.TransactionCommit, data);
            }
        }

        void WriteTransactionRollback(Guid data)
        {
            if (_diagnosticListener.IsEnabled(YsNetCoreDiagnosticListenerNames.TransactionRollback))
            {
                _diagnosticListener.Write(YsNetCoreDiagnosticListenerNames.TransactionRollback, data);
            }
        }
        #endregion
    }
}
