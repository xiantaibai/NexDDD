using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.DatabaseAccessor;

namespace NexDDD.Provider.EntityFrameworkCore
{
    /// <summary>
    /// 定义EF Core 持久化模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface ITransactionUnitOfWork : IUnitOfWork
    {
        IDbContextTransaction BeginTransaction();


        IDbContextTransaction? CurrentTransaction { get; }

        /// <summary>
        /// 提交并清除当前事务
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 回滚并清除当前事物
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
