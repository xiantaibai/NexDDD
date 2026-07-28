using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.DatabaseAccessor.Filter;

namespace NexDDD.Core.DatabaseAccessor
{
    /// <summary>
    /// 负责数据访问与聚合持久化模块中的 EntityTenant 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public abstract class EntityTenant : BaseEntity, ITenantIdFilter
    {
        public virtual long TenantId { get; set; }
    }
}
