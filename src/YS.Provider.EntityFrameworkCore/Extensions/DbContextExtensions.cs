using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        /// <summary>
        /// 动态应用排序字段列表（支持多字段、多方向）
        /// </summary>
        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> source, IEnumerable<SortField> sortFields)
        {
            if (sortFields == null || !sortFields.Any())
                return source;

            IOrderedQueryable<T>? orderedQuery = null;

            foreach (var (field, desc) in sortFields)
            {
                // 获取属性信息（忽略大小写）
                var property = typeof(T).GetProperty(field,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                    continue; // 无效字段跳过

                var parameter = Expression.Parameter(typeof(T), "x");
                var propertyAccess = Expression.Property(parameter, property);
                var orderByExp = Expression.Lambda(propertyAccess, parameter);

                string methodName;
                if (orderedQuery == null)
                {
                    // 第一个排序字段使用 OrderBy / OrderByDescending
                    methodName = desc ? "OrderByDescending" : "OrderBy";
                    orderedQuery = (IOrderedQueryable<T>)typeof(Queryable)
                        .GetMethods()
                        .First(m => m.Name == methodName
                                 && m.GetParameters().Length == 2)
                        .MakeGenericMethod(typeof(T), property.PropertyType)
                        .Invoke(null, new object[] { source, orderByExp })!;
                }
                else
                {
                    // 后续排序字段使用 ThenBy / ThenByDescending
                    methodName = desc ? "ThenByDescending" : "ThenBy";
                    orderedQuery = (IOrderedQueryable<T>)typeof(Queryable)
                        .GetMethods()
                        .First(m => m.Name == methodName
                                 && m.GetParameters().Length == 2)
                        .MakeGenericMethod(typeof(T), property.PropertyType)
                        .Invoke(null, new object[] { orderedQuery, orderByExp })!;
                }
            }

            return orderedQuery ?? source;
        }
    }
}
