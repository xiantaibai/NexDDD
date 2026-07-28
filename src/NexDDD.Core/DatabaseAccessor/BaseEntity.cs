using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.DatabaseAccessor.Filter;

namespace NexDDD.Core.DatabaseAccessor
{
    /// <summary>
    /// 负责数据访问与聚合持久化模块中的 BaseEntity 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public abstract class BaseEntity : Entity<long>, IDeletedFilter
    {
        public virtual DateTime CreateTime { get; set; }

        public virtual DateTime? LastModifyTime { get; set; }

        public virtual long? CreateUserId { get; set; }

        public virtual string? CreateUserName { get; set; }

        public virtual long? LastModifyUserId { get; set; }

        public virtual string? LastModifyUserName { get; set; }

        public virtual bool IsDelete { get; set; } = false;
    }
}
