using Microsoft.Extensions.Options;
using NexDDD.Core.Exceptions.Filters;
using NexDDD.Core.Exceptions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NexDDD.Core.Extensions;

namespace NexDDD.Core.Exceptions.Extensions
{
    /// <summary>
    /// 提供友好异常模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
    public static class FriendlyExceptionServiceCollectionExtensions
    {
        /// <summary>
        /// 添加友好异常服务拓展服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddFriendlyException(this IServiceCollection services, Action<FriendlyExceptionOptions> configure = null, string friendlyExceptionSetting = "FriendlyExceptionSettings")
        {
            // 解决服务重复注册问题
            if (services.Any(u => u.ServiceType == typeof(IConfigureOptions<FriendlyExceptionSettingsOptions>)))
            {
                return services;
            }

            using (ServiceProvider provider = services.BuildServiceProvider())
            {
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                if (configuration == null)
                {
                    throw new ArgumentNullException(nameof(IConfiguration));
                }

                // 添加配置
                services.Configure<FriendlyExceptionSettingsOptions>(configuration.GetSection(friendlyExceptionSetting));

                // 载入服务配置选项
                var configureOptions = new FriendlyExceptionOptions();
                configure?.Invoke(configureOptions);

                // 添加全局异常过滤器
                if (configureOptions.GlobalEnabled)
                    services.AddCollectionFilter<FriendlyExceptionFilter>();
            }
            // 添加异常配置文件支持
            // services.AddConfigurableOptions<ErrorCodeMessageSettingsOptions>();

            return services;
        }
    }
}
