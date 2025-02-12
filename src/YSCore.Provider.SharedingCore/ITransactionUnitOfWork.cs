using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.DatabaseAccessor;

namespace YSCore.Provider.SharedingCore
{
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
