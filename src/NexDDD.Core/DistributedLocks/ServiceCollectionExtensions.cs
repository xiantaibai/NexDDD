using Microsoft.Extensions.DependencyInjection;
using NexDDD.Core.DistributedLocks;

namespace NexDDD.Core.DistributedLocks
{
    /// <summary>
    /// 提供分布式锁模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册 IDistributedLock 的内存实现，供测试和单进程场景使用。
        /// </summary>
        public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddInMemoryDistributedLock(
            this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            // 以静态方法调用扩展，避免要求调用方额外引入命名空间。
            services = global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions
                .AddSingleton<IDistributedLock, InMemoryDistributedLock>(services);
            return services;
        }
    }
}
