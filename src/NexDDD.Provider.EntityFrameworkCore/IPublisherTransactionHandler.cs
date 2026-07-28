using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Provider.EntityFrameworkCore
{
    /// <summary>
    /// 定义EF Core 持久化模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface IPublisherTransactionHandler
    {
        IDbContextTransaction BeginTransaction(DbContext context);
    }
}
