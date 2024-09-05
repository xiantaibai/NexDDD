using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.MultiTenants
{
    /// <summary>
    /// 默认 从配置文件读
    /// </summary>
    public class DefaultTenantStore<T>(IEnumerable<T> tenants) : ITenantStore<T> where T :Tenant
    {
        //private readonly TenantSettings<T> _tenantSettings;

        //public DefaultTenantStore(IOptions<TenantSettings<T>> tenantSettings)
        //{
        //    _tenantSettings = tenantSettings.Value;
        //}

        public T Find(long Id)
        {
            return tenants.FirstOrDefault(t => t.Id == Id);
        }

        public Task<T> FindAsync(long Id)
        {
            return Task.FromResult(Find(Id));
        }
    }
}
