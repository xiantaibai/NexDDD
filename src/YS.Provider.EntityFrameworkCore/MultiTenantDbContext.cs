using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using YSCore.Provider.EntityFrameworkCore;
using YSCore.Base.DatabaseAccessor;
using YSCore.Base.DatabaseAccessor.Filter;
using YSCore.Base.MultiTenants;
using YSCore.Provider.EntityFrameworkCore.Extensions;

namespace YSCore.Provider.EntityFrameworkCore
{
    /// <summary>
    /// 多租户上下文 
    /// </summary>
    public class MultiTenantDbContext : AppDbContextBase
    {
        public long TenantId { get; set; }
        private readonly ITenantContextAccessor<Tenant> _tenantProvider;
        protected MultiTenantDbContext(DbContextOptions options, IServiceProvider provider) : base(options, provider)
        {
            _tenantProvider = GetService<ITenantContextAccessor<Tenant>>();
            TenantId = _tenantProvider == null ? 0 : _tenantProvider.TenantInfo.Id;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // 租户 过滤器
            if (TenantId == 0)
            {
                throw new ArgumentException("多租户标识为空");
            }
            modelBuilder.AddQueryFilter<ITenantIdFilter>(x => x.TenantId == TenantId);
        }

        protected internal override Task BeforeSaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            if (TenantId > 0)
            {
                foreach (var entry in ChangeTracker.Entries<ITenantIdFilter>().ToList())
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Entity.TenantId = TenantId;
                            break;
                        case EntityState.Modified:
                            entry.Entity.TenantId = TenantId;
                            break;
                    }
                }
            }

            return base.BeforeSaveEntitiesAsync(cancellationToken);
        }
    }
}
