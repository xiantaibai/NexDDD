using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.MultiTenants
{
    public interface ITenantStore<T> where T : Tenant
    {
        T Find(long Id);

        Task<T> FindAsync(long Id);
    }
}
