using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.DatabaseAccessor;

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
    }
}
