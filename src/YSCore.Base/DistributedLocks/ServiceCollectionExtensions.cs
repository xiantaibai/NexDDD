using Microsoft.Extensions.DependencyInjection;
using YSCore.Base.DistributedLocks;

namespace YSCore.Base.DistributedLocks
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers an in-memory implementation of IDistributedLock.
        /// Suitable for tests and single-process scenarios.
        /// </summary>
        public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddInMemoryDistributedLock(
            this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            // Call the extension as a static method to avoid requiring a using directive
            services = global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions
                .AddSingleton<IDistributedLock, InMemoryDistributedLock>(services);
            return services;
        }
    }
}
