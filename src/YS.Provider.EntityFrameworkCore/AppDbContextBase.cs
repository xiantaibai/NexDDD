using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.DatabaseAccessor;
using YSCore.Base.Diagnostics;
using YS.Provider.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using System.Xml;
using System.Linq.Expressions;
using YSCore.Base.DatabaseAccessor.Filter;
using Microsoft.VisualBasic;
using YSCore.Provider.EntityFrameworkCore.Extensions;

namespace YS.Provider.EntityFrameworkCore
{
    public class AppDbContextBase : DbContext, ITransactionUnitOfWork
    {
        private static readonly DiagnosticListener _diagnosticListener =
            new(YsNetCoreDiagnosticListenerNames.DiagnosticListenerName);

        private readonly IMediator _mediator;
        readonly IPublisherTransactionHandler? _publisherTransactionFactory;

        protected AppDbContextBase(DbContextOptions options, IMediator mediator, IServiceProvider provider) :
            base(options)
        {
            _mediator = mediator;
            _publisherTransactionFactory = provider.GetService<IPublisherTransactionHandler>();
        }


        //protected virtual void ConfigureStronglyTypedIdValueConverter(ModelConfigurationBuilder configurationBuilder)
        //{
        //}

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
