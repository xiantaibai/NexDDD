using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.MultiTenants
{
    /// <summary>
    /// 当前上下文租户
    /// </summary>
    public interface ITenantContextAccessor<T> where T : Tenant
    {
        /// <summary>
        /// Current tenant
        /// </summary>
        T? TenantInfo { get; set; }
    }
}
