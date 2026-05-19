using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using YSCore.Base.DatabaseAccessor;
using YSCore.Base.DatabaseAccessor.Repositories;
using YSCore.Base.LinqBuilder.Extensions;

namespace YSCore.Provider.EntityFrameworkCore.Repository
{
    public class ReadableRepository<TEntity, TDbContext> : IQueryRepository<TEntity>
        where TEntity : Entity
        where TDbContext : DbContext
    {
        protected virtual TDbContext DbContext { get; set; }

        /// <summary>
        /// 实体集合
        /// </summary>
        DbSet<TEntity> Entities { get; }

        /// <summary>
        /// 不跟踪的（脱轨）实体
        /// </summary>
        public virtual IQueryable<TEntity> DetachedEntities { get; internal set; }

        /// <summary>
        /// 查看实体类型
        /// </summary>
        public virtual IEntityType EntityType { get; internal set; }

        /// <summary>
        /// 数据库操作对象
        /// </summary>
        public virtual DatabaseFacade Database { get; internal set; }

        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public virtual DbConnection DbConnection { get; internal set; }

        /// <summary>
        /// 实体追综器
        /// </summary>
        public virtual ChangeTracker ChangeTracker { get; internal set; }

        /// <summary>
        /// 实体模型
        /// </summary>
        public virtual IModel Model { get; internal set; }

        public ReadableRepository(TDbContext context) 
        {
            DbContext = context;

            // 初始化数据库相关数据
            Database = DbContext?.Database;

            // 只有关系型数据库才有连接信息
            if (Database?.IsRelational() == true) DbConnection = Database?.GetDbConnection();

            ChangeTracker = DbContext?.ChangeTracker;
            Model = DbContext?.Model;

            //初始化实体
            Entities = DbContext?.Set<TEntity>();
            DetachedEntities = Entities?.AsNoTracking();
            EntityType = Entities?.EntityType;
        }

        /// <summary>
        /// 根据键查询一条记录
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>数据库中的实体</returns>
        public virtual TEntity Find(object key)
        {
            var entity = FindOrDefault(key) ?? throw new InvalidOperationException("Sequence contains no elements.");
            return entity;
        }

        /// <summary>
        /// 根据多个键查询一条记录
        /// </summary>
        /// <param name="keyValues">多个键</param>
        /// <returns>数据库中的实体</returns>
        public virtual TEntity Find(params object[] keyValues)
        {
            var entity = FindOrDefault(keyValues) ?? throw new InvalidOperationException("Sequence contains no elements.");
            return entity;
        }

        /// <summary>
        /// 根据键查询一条记录
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库实体</returns>
        public virtual async Task<TEntity> FindAsync(object key, CancellationToken cancellationToken = default)
        {
            var entity = await FindOrDefaultAsync(key, cancellationToken);
            return entity ?? throw new InvalidOperationException("Sequence contains no elements.");
        }

        /// <summary>
        /// 根据多个键查询一条记录
        /// </summary>
        /// <param name="keyValues">多个键</param>
        /// <returns>数据库中的实体</returns>
        public virtual async Task<TEntity> FindAsync(params object[] keyValues)
        {
            var entity = await FindOrDefaultAsync(keyValues);
            return entity ?? throw new InvalidOperationException("Sequence contains no elements.");
        }

        /// <summary>
        /// 根据多个键查询一条记录
        /// </summary>
        /// <param name="keyValues">多个键</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual async Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken = default)
        {
            var entity = await FindOrDefaultAsync(keyValues, cancellationToken);
            return entity ?? throw new InvalidOperationException("Sequence contains no elements.");
        }

        /// <summary>
        /// 根据键查询一条记录
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>数据库中的实体</returns>
        public virtual TEntity FindOrDefault(object key)
        {
            return Entities.Find(key);
        }

        /// <summary>
        /// 根据多个键查询一条记录
        /// </summary>
        /// <param name="keyValues">多个键</param>
        /// <returns>数据库中的实体</returns>
        public virtual TEntity FindOrDefault(params object[] keyValues)
        {
            return Entities.Find(keyValues);
        }

        /// <summary>
        /// 根据键查询一条记录
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual async Task<TEntity> FindOrDefaultAsync(object key, CancellationToken cancellationToken = default)
        {
            var entity = await Entities.FindAsync(new object[] { key }, cancellationToken);
            return entity;
        }

        /// <summary>
        /// 根据多个键查询一条记录
        /// </summary>
        /// <param name="keyValues">多个键</param>
        /// <returns>数据库中的实体</returns>
        public virtual async Task<TEntity> FindOrDefaultAsync(params object[] keyValues)
        {
            var entity = await Entities.FindAsync(keyValues);
            return entity;
        }

        /// <summary>
        /// 根据多个键查询一条记录
        /// </summary>
        /// <param name="keyValues">多个键</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual async Task<TEntity> FindOrDefaultAsync(object[] keyValues, CancellationToken cancellationToken = default)
        {
            var entity = await Entities.FindAsync(keyValues, cancellationToken);
            return entity;
        }

        /// <summary>
        /// 查询一条记录
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <returns>数据库中的实体</returns>
        public virtual TEntity First(bool? tracking = null)
        {
            return AsQueryable(tracking).First();
        }

        /// <summary>
        /// 根据表达式查询一条记录
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <returns>数据库中的实体</returns>
        public virtual TEntity First(Expression<Func<TEntity, bool>> predicate, bool? tracking = null)
        {
            return AsQueryable(tracking).First(predicate);
        }

        /// <summary>
        /// 查询一条记录
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <returns>数据库中的实体</returns>
        public virtual TEntity FirstOrDefault(bool? tracking = null)
        {
            return AsQueryable(tracking).FirstOrDefault();
        }

        /// <summary>
        /// 根据表达式查询一条记录
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <returns>数据库中的实体</returns>
        public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate, bool? tracking = null)
        {
            return AsQueryable(tracking).FirstOrDefault(predicate);
        }

        /// <summary>
        /// 查询一条记录
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<TEntity> FirstAsync(bool? tracking = null, CancellationToken cancellationToken = default)
        {
            return AsQueryable(tracking).FirstAsync(cancellationToken);
        }

        /// <summary>
        /// 根据表达式查询一条记录
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate, bool? tracking = null, CancellationToken cancellationToken = default)
        {
            return AsQueryable(tracking).FirstAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// 查询一条记录
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<TEntity> FirstOrDefaultAsync(bool? tracking = null, CancellationToken cancellationToken = default)
        {
            return AsQueryable(tracking).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// 根据表达式查询一条记录
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool? tracking = null, CancellationToken cancellationToken = default)
        {
            return AsQueryable(tracking).FirstOrDefaultAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// 查询一条记录
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <returns>数据库中的实体</returns>
        public virtual TEntity Last(bool? tracking = null)
        {
            return AsQueryable(tracking).Last();
        }

        /// <summary>
        /// 根据表达式查询一条记录
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <returns>数据库中的实体</returns>
        public virtual TEntity Last(Expression<Func<TEntity, bool>> predicate, bool? tracking = null)
        {
            return AsQueryable(tracking).Last(predicate);
        }

        /// <summary>
        /// 查询一条记录
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <returns>数据库中的实体</returns>
        public virtual TEntity LastOrDefault(bool? tracking = null)
        {
            return AsQueryable(tracking).LastOrDefault();
        }

        /// <summary>
        /// 根据表达式查询一条记录
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <returns>数据库中的实体</returns>
        public virtual TEntity LastOrDefault(Expression<Func<TEntity, bool>> predicate, bool? tracking = null)
        {
            return AsQueryable(tracking).LastOrDefault(predicate);
        }

        /// <summary>
        /// 查询一条记录
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<TEntity> LastAsync(bool? tracking = null, CancellationToken cancellationToken = default)
        {
            return AsQueryable(tracking).LastAsync(cancellationToken);
        }

        /// <summary>
        /// 根据表达式查询一条记录
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<TEntity> LastAsync(Expression<Func<TEntity, bool>> predicate, bool? tracking = null, CancellationToken cancellationToken = default)
        {
            return AsQueryable(tracking).LastAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// 查询一条记录
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<TEntity> LastOrDefaultAsync(bool? tracking = null, CancellationToken cancellationToken = default)
        {
            return AsQueryable(tracking).LastOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// 根据表达式查询一条记录
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<TEntity> LastOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool? tracking = null, CancellationToken cancellationToken = default)
        {
            return AsQueryable(tracking).LastOrDefaultAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// 根据表达式查询多条记录
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>数据库中的多个实体</returns>
        public virtual IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(predicate, tracking, ignoreQueryFilters);
        }

        /// <summary>
        /// 根据表达式查询多条记录
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>数据库中的多个实体</returns>
        public virtual IQueryable<TEntity> Where(Expression<Func<TEntity, int, bool>> predicate, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(predicate, tracking, ignoreQueryFilters);
        }

        /// <summary>
        /// 根据条件执行表达式查询多条记录
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>数据库中的多个实体</returns>
        public virtual IQueryable<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> predicate, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).Where(condition, predicate);
        }

        /// <summary>
        /// 根据条件执行表达式查询多条记录
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>数据库中的多个实体</returns>
        public virtual IQueryable<TEntity> Where(bool condition, Expression<Func<TEntity, int, bool>> predicate, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, int, bool>>), tracking, ignoreQueryFilters).Where(condition, predicate);
        }

        /// <summary>
        /// 根据条件执行表达式查询多条记录
        /// </summary>
        /// <param name="predicates">表达式集合</param>
        /// <returns>数据库中的多个实体</returns>
        public virtual IQueryable<TEntity> Where(params Expression<Func<TEntity, bool>>[] predicates)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>)).Where(predicates);
        }

        /// <summary>
        /// 根据条件执行表达式查询多条记录
        /// </summary>
        /// <param name="predicates">表达式集合</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>数据库中的多个实体</returns>
        public virtual IQueryable<TEntity> Where(Expression<Func<TEntity, bool>>[] predicates, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).Where(predicates);
        }

        /// <summary>
        /// 根据条件执行表达式查询多条记录
        /// </summary>
        /// <param name="predicates">表达式集合</param>
        /// <returns>数据库中的多个实体</returns>
        public virtual IQueryable<TEntity> Where(params Expression<Func<TEntity, int, bool>>[] predicates)
        {
            return AsQueryable(default(Expression<Func<TEntity, int, bool>>)).Where(predicates);
        }

        /// <summary>
        /// 根据条件执行表达式查询多条记录
        /// </summary>
        /// <param name="predicates">表达式集合</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>数据库中的多个实体</returns>
        public virtual IQueryable<TEntity> Where(Expression<Func<TEntity, int, bool>>[] predicates, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, int, bool>>), tracking, ignoreQueryFilters).Where(predicates);
        }

        /// <summary>
        /// 根据条件执行表达式查询多条记录
        /// </summary>
        /// <param name="conditionPredicates">条件表达式集合</param>
        /// <returns>数据库中的多个实体</returns>
        public virtual IQueryable<TEntity> Where(params (bool condition, Expression<Func<TEntity, bool>> expression)[] conditionPredicates)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>)).Where(conditionPredicates);
        }

        /// <summary>
        /// 根据条件执行表达式查询多条记录
        /// </summary>
        /// <param name="conditionPredicates">条件表达式集合</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>数据库中的多个实体</returns>
        public virtual IQueryable<TEntity> Where((bool condition, Expression<Func<TEntity, bool>> expression)[] conditionPredicates, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).Where(conditionPredicates);
        }

        /// <summary>
        /// 根据条件执行表达式查询多条记录
        /// </summary>
        /// <param name="conditionPredicates">条件表达式集合</param>
        /// <returns>数据库中的多个实体</returns>
        public virtual IQueryable<TEntity> Where(params (bool condition, Expression<Func<TEntity, int, bool>> expression)[] conditionPredicates)
        {
            return AsQueryable(default(Expression<Func<TEntity, int, bool>>)).Where(conditionPredicates);
        }

        /// <summary>
        /// 根据条件执行表达式查询多条记录
        /// </summary>
        /// <param name="conditionPredicates">条件表达式集合</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>数据库中的多个实体</returns>
        public virtual IQueryable<TEntity> Where((bool condition, Expression<Func<TEntity, int, bool>> expression)[] conditionPredicates, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, int, bool>>), tracking, ignoreQueryFilters).Where(conditionPredicates);
        }

        /// <summary>
        /// 判断记录是否存在
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>bool</returns>
        public virtual bool Any(bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).Any();
        }

        /// <summary>
        /// 根据表达式判断记录是否存在
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>bool</returns>
        public virtual bool Any(Expression<Func<TEntity, bool>> predicate, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).Any(predicate);
        }

        /// <summary>
        /// 判断记录是否存在
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>bool</returns>
        public virtual Task<bool> AnyAsync(bool? tracking = null, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).AnyAsync(cancellationToken);
        }

        /// <summary>
        /// 根据表达式判断记录是否存在
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>bool</returns>
        public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, bool? tracking = null, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).AnyAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// 查看记录条数
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>int</returns>
        public virtual int Count(bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).Count();
        }

        /// <summary>
        /// 根据表达式查询记录条数
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>int</returns>
        public virtual int Count(Expression<Func<TEntity, bool>> predicate, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).Count(predicate);
        }

        /// <summary>
        /// 查看记录条数
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>int</returns>
        public virtual Task<int> CountAsync(bool? tracking = null, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).CountAsync(cancellationToken);
        }

        /// <summary>
        /// 根据表达式查询记录条数
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>int</returns>
        public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, bool? tracking = null, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).CountAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// 查看最小记录
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>实体</returns>
        public virtual TEntity Min(bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).Min();
        }

        /// <summary>
        /// 根据表达式查看最小值
        /// </summary>
        /// <typeparam name="TResult">最小值类型</typeparam>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>TResult</returns>
        public virtual TResult Min<TResult>(Expression<Func<TEntity, TResult>> predicate, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).Min(predicate);
        }

        /// <summary>
        /// 查看最小记录
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>实体</returns>
        public virtual Task<TEntity> MinAsync(bool? tracking = null, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).MinAsync(cancellationToken);
        }

        /// <summary>
        /// 根据表达式查看最小值
        /// </summary>
        /// <typeparam name="TResult">最小值类型</typeparam>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>TResult</returns>
        public virtual Task<TResult> MinAsync<TResult>(Expression<Func<TEntity, TResult>> predicate, bool? tracking = null, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).MinAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// 查看最大记录
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>实体</returns>
        public virtual TEntity Max(bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).Max();
        }

        /// <summary>
        /// 根据表达式查看最大值
        /// </summary>
        /// <typeparam name="TResult">最大值类型</typeparam>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>TResult</returns>
        public virtual TResult Max<TResult>(Expression<Func<TEntity, TResult>> predicate, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).Max(predicate);
        }

        /// <summary>
        /// 查看最大记录
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>实体</returns>
        public virtual Task<TEntity> MaxAsync(bool? tracking = null, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).MaxAsync(cancellationToken);
        }

        /// <summary>
        /// 根据表达式查看最大值
        /// </summary>
        /// <typeparam name="TResult">最大值类型</typeparam>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>TResult</returns>
        public virtual Task<TResult> MaxAsync<TResult>(Expression<Func<TEntity, TResult>> predicate, bool? tracking = null, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default)
        {
            return AsQueryable(default(Expression<Func<TEntity, bool>>), tracking, ignoreQueryFilters).MaxAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// 构建查询分析器
        /// </summary>
        /// <param name="tracking">是否跟踪实体</param>
        /// <returns>IQueryable{TEntity}</returns>
        public virtual IQueryable<TEntity> AsQueryable(bool? tracking = null)
        {
            // 启用实体跟踪
            var isTracking = tracking ?? false;

            return isTracking ? Entities : DetachedEntities;
        }

        /// <summary>
        /// 构建查询分析器
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>IQueryable{TEntity}</returns>
        public virtual IQueryable<TEntity> AsQueryable(Expression<Func<TEntity, bool>> predicate, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            var entities = AsQueryable(tracking);
            if (ignoreQueryFilters) entities = entities.IgnoreQueryFilters();
            if (predicate != null) entities = entities.Where(predicate);

            return entities;
        }

        /// <summary>
        /// 构建查询分析器
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <param name="tracking">是否跟踪实体</param>
        /// <param name="ignoreQueryFilters">是否忽略查询过滤器</param>
        /// <returns>IQueryable{TEntity}</returns>
        public virtual IQueryable<TEntity> AsQueryable(Expression<Func<TEntity, int, bool>> predicate, bool? tracking = null, bool ignoreQueryFilters = false)
        {
            var entities = AsQueryable(tracking);
            if (ignoreQueryFilters) entities = entities.IgnoreQueryFilters();
            if (predicate != null) entities = entities.Where(predicate);

            return entities;
        }

        /// <summary>
        /// 执行 Sql 返回 IQueryable
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>IQueryable</returns>
        public virtual IQueryable<TEntity> FromSqlRaw(string sql, params object[] parameters)
        {
            // 支持读取配置渲染
            // var realSql = sql.Render();

            return Entities.FromSqlRaw(sql, parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 IQueryable
        /// </summary>
        /// <remarks>
        /// 支持字符串内插语法
        /// </remarks>
        /// <param name="sql">sql 语句</param>
        /// <returns>IQueryable</returns>
        public virtual IQueryable<TEntity> FromSqlInterpolated(FormattableString sql)
        {
            return Entities.FromSqlInterpolated(sql);
        }
    }
}
