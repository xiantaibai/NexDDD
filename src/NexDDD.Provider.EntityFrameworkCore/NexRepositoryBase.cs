using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.DatabaseAccessor;
using NexDDD.Core.DatabaseAccessor.Repositories;
using NexDDD.Provider.EntityFrameworkCore.Repository;

namespace NexDDD.Provider.EntityFrameworkCore
{
    /// <summary>
    /// 实现EF Core 持久化模块的数据访问边界，为上层业务提供一致的查询与持久化能力。
    /// </summary>
    public abstract class NexRepositoryBase<TEntity, TDbContext> : ReadableRepository<TEntity, TDbContext>, INexRepository<TEntity>
        where TEntity : Entity, IAggregateRoot 
        where TDbContext : DbContext, IUnitOfWork
    {
        //protected virtual TDbContext DbContext { get; set; }
        protected NexRepositoryBase(TDbContext context) : base(context)
        {
        }

        public virtual IUnitOfWork UnitOfWork => DbContext;

        /// <summary>
        /// 写仓储默认用于加载聚合并修改，所以默认启用 EF Core tracking。
        /// 需要只读查询时，可以显式传入 tracking: false。
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <returns>IQueryable{TEntity}</returns>
        public override IQueryable<TEntity> AsQueryable(bool? tracking = null)
        {
            return base.AsQueryable(tracking ?? true);
        }

        public virtual TEntity Add(TEntity entity) => DbContext.Add(entity).Entity;

        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var entry = await DbContext.AddAsync(entity, cancellationToken);
            return entry.Entity;
        }

        public virtual TEntity Update(TEntity entity) => DbContext.Update(entity).Entity;

        public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) =>
            Task.FromResult(Update(entity));

        public virtual bool Remove(Entity entity)
        {
            DbContext.Remove(entity);
            return true;
        }

        public virtual Task<bool> RemoveAsync(Entity entity) => Task.FromResult(Remove(entity));
    }
    /// <summary>
    /// 实现带强类型主键的聚合写仓储，为应用层提供按主键加载、删除及软删除能力。
    /// </summary>
    public abstract class
        NexRepositoryBase<TEntity, TKey, TDbContext> : NexRepositoryBase<TEntity, TDbContext>, INexRepository<TEntity, TKey>
        where TEntity : Entity<TKey>, IAggregateRoot where TDbContext : DbContext, IUnitOfWork where TKey : notnull
    {
        protected NexRepositoryBase(TDbContext context) : base(context)
        {
        }

        public virtual int DeleteById(TKey id)
        {
            return DbContext.Set<TEntity>().Where(p => p.Id.Equals(id)).ExecuteDelete();
        }

        public virtual async Task<int> DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<TEntity>().Where(p => p.Id.Equals(id)).ExecuteDeleteAsync(cancellationToken);
        }

        public virtual int FakeDeleteById(TKey id)
        {
            var entity = Get(id);
            if (entity != null)
            {
                var property = typeof(TEntity).GetProperty("IsDelete");
                if (property != null && property.CanWrite)
                {
                    property.SetValue(entity, true);
                    return 1;
                }
            }
            return 0;
        }

        public virtual async Task<int> FakeDeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await GetAsync(id);
            if (entity != null)
            {
                var property = typeof(TEntity).GetProperty("IsDelete");
                if (property != null && property.CanWrite)
                {
                    property.SetValue(entity, true);
                    return 1;
                }
            }
            return 0;
        }

        public virtual TEntity? Get(TKey id) => DbContext.Find<TEntity>(id);

        public virtual async Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await DbContext.FindAsync<TEntity>(new object[] { id }, cancellationToken);
        }
    }
}
