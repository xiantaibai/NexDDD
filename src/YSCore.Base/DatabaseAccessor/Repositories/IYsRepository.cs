using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.DatabaseAccessor.Repositories
{
    /// <summary>
    /// 聚合写侧读取接口。
    /// 用于 Command 处理过程中从主库加载聚合根，服务后续领域行为和写入，不表示读模型查询仓储。
    /// </summary>
    /// <typeparam name="TEntity">聚合根类型</typeparam>
    public interface IAggregateReadableRepository<TEntity> : IPrivateReadableRepository<TEntity>
        where TEntity : notnull, Entity, IAggregateRoot
    {
    }

    /// <summary>
    /// 聚合写仓储接口，必须面向聚合根。
    /// 写仓储可以具备聚合加载能力，但语义上始终属于写侧主库仓储。
    /// </summary>
    /// <typeparam name="TEntity">聚合根类型</typeparam>
    public interface IYsRepository<TEntity> : IAggregateReadableRepository<TEntity>
        where TEntity : notnull, Entity, IAggregateRoot
    {
        /// <summary>
        /// 获取工作单元对象。
        /// </summary>
        IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// 添加一个实体到仓储。
        /// </summary>
        /// <param name="entity">要添加的实体对象</param>
        /// <returns>添加后的实体对象</returns>
        TEntity Add(TEntity entity);

        /// <summary>
        /// 异步添加一个实体到仓储。
        /// </summary>
        /// <param name="entity">要添加的实体对象</param>
        /// <param name="cancellationToken">取消操作 token</param>
        /// <returns>添加后的实体对象</returns>
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新实体。
        /// </summary>
        /// <param name="entity">要更新的实体对象</param>
        /// <returns>更新后的实体对象</returns>
        TEntity Update(TEntity entity);

        /// <summary>
        /// 异步更新实体。
        /// </summary>
        /// <param name="entity">要更新的实体对象</param>
        /// <param name="cancellationToken">取消操作 token</param>
        /// <returns>更新后的实体对象</returns>
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 删除实体。
        /// </summary>
        /// <param name="entity">要删除的实体对象</param>
        /// <returns>是否删除成功</returns>
        bool Remove(Entity entity);

        /// <summary>
        /// 异步删除实体。
        /// </summary>
        /// <param name="entity">要删除的实体对象</param>
        /// <returns>是否删除成功</returns>
        Task<bool> RemoveAsync(Entity entity);
    }

    /// <summary>
    /// 带主键类型的聚合写仓储接口。
    /// </summary>
    /// <typeparam name="TEntity">聚合根类型</typeparam>
    /// <typeparam name="TKey">主键类型</typeparam>
    public interface IYsRepository<TEntity, TKey> : IYsRepository<TEntity>
        where TEntity : notnull, Entity<TKey>, IAggregateRoot
        where TKey : notnull
    {
        /// <summary>
        /// 根据主键删除实体。
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns>影响行数</returns>
        int DeleteById(TKey id);

        /// <summary>
        /// 根据主键软删除实体。
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns>影响行数</returns>
        int FakeDeleteById(TKey id);

        /// <summary>
        /// 根据主键异步删除实体。
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="cancellationToken">取消操作 token</param>
        /// <returns>影响行数</returns>
        Task<int> DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据主键异步软删除实体。
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="cancellationToken">取消操作 token</param>
        /// <returns>影响行数</returns>
        Task<int> FakeDeleteByIdAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据主键获取实体。
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns>实体对象</returns>
        TEntity? Get(TKey id);

        /// <summary>
        /// 根据主键异步获取实体。
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="cancellationToken">取消操作 token</param>
        /// <returns>实体对象</returns>
        Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default);
    }
}
