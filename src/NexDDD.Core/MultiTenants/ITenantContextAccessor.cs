using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.MultiTenants
{
    /// <summary>
    /// 当前上下文租户
    /// 定义多租户模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface ITenantContextAccessor<T> where T : Tenant
    {
        /// <summary>
        /// 获取或设置当前调用链解析出的租户。
        /// </summary>
        T? TenantInfo { get; set; }
    }
}
