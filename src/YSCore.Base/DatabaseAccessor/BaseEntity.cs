using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.DatabaseAccessor.Filter;

namespace YSCore.Base.DatabaseAccessor
{
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
