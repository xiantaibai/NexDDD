using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.DatabaseAccessor.SeedData
{
    /// <summary>
    /// 种子数据
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
