using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.DatabaseAccessor.Filter;

namespace YSCore.Base.DatabaseAccessor
{
    public abstract class EntityTenant : BaseEntity, ITenantIdFilter
    {
        public virtual long TenantId { get; set; }
    }
}
