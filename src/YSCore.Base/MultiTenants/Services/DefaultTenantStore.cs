using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.MultiTenants
{
    /// <summary>
    /// 默认 从配置文件读
    /// </summary>
    public class DefaultTenantStore<T> : ITenantStore<T> where T :Tenant
    {
        private readonly TenantSettings<T> _tenantSettings;
        private readonly ConcurrentDictionary<long, T> _tenantMap;

        public DefaultTenantStore(IOptions<TenantSettings<T>> tenantSettings)
        {
            _tenantSettings = tenantSettings.Value;

            _tenantMap = new ConcurrentDictionary<long, T>();
            foreach (var tenant in _tenantSettings.Tenants)
            {
                _tenantMap.TryAdd(tenant.Id, tenant);
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Task.FromResult(_tenantSettings.Tenants);
        }

        public async Task<bool> TryAddAsync(T tenantInfo)
        {
            var result = tenantInfo?.Id != null && _tenantMap.TryAdd(tenantInfo.Id, tenantInfo);

            return await Task.FromResult(result);
        }

        public async Task<T?> TryGetAsync(long id)
        {
            _tenantMap.TryGetValue(id, out var result);

            return await Task.FromResult(result);
        }

        public async Task<bool> TryRemoveAsync(long id)
        {
            var result = _tenantMap.TryRemove(id, out var _);

            return await Task.FromResult(result);
        }

        public async Task<bool> TryUpdateAsync(T tenantInfo)
        {
            var existingTenantInfo = tenantInfo?.Id != null ? await TryGetAsync(tenantInfo.Id) : null;

            if (existingTenantInfo != null)
            {
                var result = _tenantMap.TryUpdate(existingTenantInfo.Id, tenantInfo, existingTenantInfo);
                return await Task.FromResult(result);
            }

            return await Task.FromResult(false);
        }
    }
}
