using YSCore.Base.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace YSCore.Base.Context
{
    public static class ContextExtensions
    {
        public static IServiceCollection AddContext<TContext>(this IServiceCollection services,
            Action<ContextOptions<TContext>>? configureOptions = null) where TContext : class
        {
            var options = new ContextOptions<TContext>();
            configureOptions?.Invoke(options);

            // 注册上下文工厂
            if (options.ContextFactory != null)
            {
                services.AddSingleton<IContextFactory<TContext>>(options.ContextFactory);
            }
            else
            {
                services.AddSingleton<IContextFactory<TContext>, DefaultContextFactory<TContext>>();
            }

            // 注册序列化器
            services.AddSingleton<IContextSerializer>(options.Serializer ?? new JsonContextSerializer());

            // 注册上下文访问器
            services.AddSingleton<IContextAccessor<TContext>>(sp =>
            {
                var factory = sp.GetRequiredService<IContextFactory<TContext>>();
                var serializer = sp.GetRequiredService<IContextSerializer>();
                var logger = sp.GetService<ILogger<ContextAccessor<TContext>>>();
                return new ContextAccessor<TContext>(factory, serializer, logger);
            });

            return services;
        }
    }
}