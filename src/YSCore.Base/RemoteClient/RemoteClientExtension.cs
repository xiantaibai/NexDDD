using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.RemoteClient
{
    public static class RemoteClientExtension
    {
        public static IServiceCollection AddRemoteRequest(this IServiceCollection services, Action<IServiceCollection> configure = null)
        {
            // 注册其他客户端
            configure?.Invoke(services);

            return services;
        }

        public static IHttpClientBuilder AddHttpClientWithOptions<TClient>(this IServiceCollection services, string clientName, Action<RemoteClientOptions> configureOptions)
        where TClient : class
        {
            // 注册配置选项
            services.Configure<RemoteClientOptions>(clientName, configureOptions);

            // 注册 HttpClient 及其实现类
            return services.AddHttpClient<TClient>((serviceProvider, httpClient) =>
             {
                 var options = serviceProvider.GetRequiredService<IOptionsMonitor<RemoteClientOptions>>().Get(clientName);
                 httpClient.Timeout = options.Timeout;

                 // 可以根据需要配置 HttpClient 的其他属性，比如 BaseAddress 等
                 if (!string.IsNullOrEmpty(options.BaseAddress))
                 {
                     httpClient.BaseAddress = new Uri(options.BaseAddress);
                 }
             })
             // 设置客户端生存期为 5 分钟
             .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        }
    }
}
