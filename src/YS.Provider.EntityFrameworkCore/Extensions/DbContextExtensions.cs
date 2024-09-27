using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.DatabaseAccessor;
using YSCore.Base.Pagelist;

namespace YSCore.Provider.EntityFrameworkCore.Extensions
{
    public static class DbContextExtensions
    {
        // 扩展方法：软删除实体
        public static async Task FakeDeleteAsync<TEntity>(this DbContext context, params object?[]? keyValues) where TEntity : BaseEntity
        {
            var entity = await context.Set<TEntity>().FindAsync(keyValues);
            if (entity != null)
            {
                var property = typeof(TEntity).GetProperty("IsDelete");
                if (property != null && property.CanWrite)
                {
                    property.SetValue(entity, true);
                }
            }
        }
        public static async Task FakeDeleteAsync<TEntity>(this DbSet<TEntity> dbSet, params object?[]? keyValues) where TEntity : BaseEntity
        {
            var entity = await dbSet.FindAsync(keyValues);
            if (entity != null)
            {
                var property = typeof(TEntity).GetProperty("IsDelete");
                if (property != null && property.CanWrite)
                {
                    property.SetValue(entity, true);
                }
            }
        }

        public static async Task FakeDeleteAsync<TEntity>(this IQueryable<TEntity> source) where TEntity : BaseEntity
        {
            foreach (var entity in source)
            {
                var property = typeof(TEntity).GetProperty("IsDelete");
                if (property != null && property.CanWrite)
                {
                    property.SetValue(entity, true);
                }
            }
        }

        public static PagedList<T> ToPagedList<T>(this List<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count;
            var items = source.Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToList();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
