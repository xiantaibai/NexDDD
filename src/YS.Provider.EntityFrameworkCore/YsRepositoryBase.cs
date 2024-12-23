using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.DatabaseAccessor;
using YSCore.Base.DatabaseAccessor.Repositories;
using YSCore.Provider.EntityFrameworkCore.Repository;

namespace YS.Provider.EntityFrameworkCore
{
    public abstract class YsRepositoryBase<TEntity, TDbContext> : ReadableRepository<TEntity, TDbContext>, IYsRepository<TEntity>
        where TEntity : Entity, IAggregateRoot 
        where TDbContext : DbContext, IUnitOfWork
    {
        //protected virtual TDbContext DbContext { get; set; }
        protected YsRepositoryBase(TDbContext context) : base(context)
        {
        }

        public virtual IUnitOfWork UnitOfWork => DbContext;

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


    public abstract class
        YsRepositoryBase<TEntity, TKey, TDbContext> : YsRepositoryBase<TEntity, TDbContext>, IYsRepository<TEntity, TKey>
        where TEntity : Entity<TKey>, IAggregateRoot where TDbContext : DbContext, IUnitOfWork where TKey : notnull
    {
        protected YsRepositoryBase(TDbContext context) : base(context)
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
