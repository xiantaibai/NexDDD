using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.MultiTenants
{
    /// <summary>
    /// 租户解析
    /// </summary>
    public interface ITenantResolver
    {
        long GetTenantId();
    }
}
