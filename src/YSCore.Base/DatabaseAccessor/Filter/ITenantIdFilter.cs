using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.DatabaseAccessor.Filter
{
    /// <summary>
    /// 租户Id接口过滤器
    /// </summary>
    public interface ITenantIdFilter
    {
        /// <summary>
        /// 租户Id
        /// </summary>
        long TenantId { get; set; }
    }
}
