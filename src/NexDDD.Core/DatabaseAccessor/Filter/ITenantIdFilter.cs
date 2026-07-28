using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.DatabaseAccessor.Filter
{
    /// <summary>
    /// 租户Id接口过滤器
    /// 定义数据访问与聚合持久化模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface ITenantIdFilter
    {
        /// <summary>
        /// 租户Id
        /// </summary>
        long TenantId { get; set; }
    }
}
