using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.MultiTenants
{
    /// <summary>
    /// 定义多租户模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface ITenantStore<T> where T : Tenant
    {
        Task<bool> TryAddAsync(T tenantInfo);

        
        Task<bool> TryUpdateAsync(T tenantInfo);

        
        Task<bool> TryRemoveAsync(long id);


        /// <summary>
        /// 根据Id 获取所有租户
        /// </summary>
        Task<T?> TryGetAsync(long id);


        /// <summary>
        /// 获取所有的对象.
        /// </summary>
        /// <returns>存储所有的租户.</returns>
        Task<IEnumerable<T>> GetAllAsync();
    }
}
