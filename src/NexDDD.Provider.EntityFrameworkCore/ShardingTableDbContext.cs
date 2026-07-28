using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using MediatR;
using NexDDD.Core.DatabaseAccessor.Filter;
using Microsoft.EntityFrameworkCore.Storage;
using NexDDD.Core.DatabaseAccessor;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using NexDDD.Core.Diagnostics;
using NexDDD.Provider.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using NexDDD.Provider.EntityFrameworkCore.Idempotency;

namespace NexDDD.Provider.EntityFrameworkCore
{
    /// <summary>
    /// 分表上下文
    /// 负责EF Core 持久化模块中的 ShardingTableDbContext 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    public class ShardingTableDbContext : AbstractShardingDbContext, IShardingTableDbContext, ITransactionUnitOfWork
    {
        private static readonly DiagnosticListener _diagnosticListener =
            new(NexDddDiagnosticListenerNames.DiagnosticListenerName);

        public IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;
        private readonly IPublisherTransactionHandler? _publisherTransactionFactory;
        public ShardingTableDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options)
        {
            _serviceProvider = serviceProvider;
            //_serviceProvider = GetService<IServiceProvider>();
            _mediator = GetService<IMediator>();
            _publisherTransactionFactory = GetService<IPublisherTransactionHandler>();
        }

        // 获取服务
        public T GetService<T>() where T : class
            => _serviceProvider.GetService<T>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IsDelete 过滤器
            modelBuilder.AddQueryFilter<IDeletedFilter>(x => x.IsDelete == false);
            modelBuilder.AddIdempotencyModel();
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            configurationBuilder.Properties<string>()
                                .HaveMaxLength(256);

            configurationBuilder
               .Properties<decimal>()
               .HavePrecision(18, 2);

            configurationBuilder
                .Properties<decimal?>()
                .HavePrecision(18, 2);
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
                // await CurrentTransaction.CommitAsync(cancellationToken);
                CurrentTransaction.Commit();
                WriteTransactionCommit(CurrentTransaction.TransactionId);
                CurrentTransaction = null;
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (CurrentTransaction != null)
            {
                // await CurrentTransaction.RollbackAsync(cancellationToken);
                CurrentTransaction.Rollback();
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
                        // await base.SaveChangesAsync(cancellationToken);
                        base.SaveChanges();
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
                // await base.SaveChangesAsync(cancellationToken);
                base.SaveChanges();
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
            if (_diagnosticListener.IsEnabled(NexDddDiagnosticListenerNames.TransactionBegin))
            {
                _diagnosticListener.Write(NexDddDiagnosticListenerNames.TransactionBegin, data);
            }
        }

        void WriteTransactionCommit(Guid data)
        {
            if (_diagnosticListener.IsEnabled(NexDddDiagnosticListenerNames.TransactionCommit))
            {
                _diagnosticListener.Write(NexDddDiagnosticListenerNames.TransactionCommit, data);
            }
        }

        void WriteTransactionRollback(Guid data)
        {
            if (_diagnosticListener.IsEnabled(NexDddDiagnosticListenerNames.TransactionRollback))
            {
                _diagnosticListener.Write(NexDddDiagnosticListenerNames.TransactionRollback, data);
            }
        }
        #endregion
    }
}
