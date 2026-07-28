using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace testApi.MultiTenantDbContext;

/// <summary>
/// 用于验证请求上下文相关行为，确保框架重命名后契约和运行结果保持正确。
/// </summary>
public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        throw new NotImplementedException();
    }
}