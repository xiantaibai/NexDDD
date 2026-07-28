using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.DatabaseAccessor.SeedData
{
    /// <summary>
    /// 种子数据
    /// 定义数据访问与聚合持久化模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IEntitySeedData<TEntity>
        where TEntity : Entity
    {
        /// <summary>
        /// 种子数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<TEntity> HasData();
    }
}
