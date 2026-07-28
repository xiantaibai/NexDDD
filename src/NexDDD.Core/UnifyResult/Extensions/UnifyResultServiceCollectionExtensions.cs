using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Reflection;
using NexDDD.Core.Extensions;
using NexDDD.Core.UnifyResult.Attributes;
using NexDDD.Core.UnifyResult.Filters;
using NexDDD.Core.UnifyResult.Internal;
using NexDDD.Core.UnifyResult.Options;
using NexDDD.Core.UnifyResult.Providers;

namespace NexDDD.Core.UnifyResult.Extensions
{
    /// <summary>
    /// 规范化结果服务拓展
    /// </summary>
    public static class UnifyResultServiceCollectionExtensions
    {
        /// <summary>
        /// 添加规范化结果服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUnifyResult(this IServiceCollection services)
        {
            services.AddUnifyResult<RESTfulResultProvider>();

            return services;
        }

        /// <summary>
        /// 添加规范化结果服务
        /// </summary>
        /// <typeparam name="TUnifyResultProvider"></typeparam>
        /// <param name="mvcBuilder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddUnifyResult<TUnifyResultProvider>(this IMvcBuilder mvcBuilder)
            where TUnifyResultProvider : class, IUnifyResultProvider
        {
            mvcBuilder.Services.AddUnifyResult<TUnifyResultProvider>();

            return mvcBuilder;
        }

        /// <summary>
        /// 添加规范化结果服务
        /// </summary>
        /// <typeparam name="TUnifyResultProvider"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUnifyResult<TUnifyResultProvider>(this IServiceCollection services, string section = "UnifyResultSettings")
            where TUnifyResultProvider : class, IUnifyResultProvider
        {
            // 解决服务重复注册问题
            if (services.Any(u => u.ServiceType == typeof(IConfigureOptions<UnifyResultSettingsOptions>)))
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
                services.Configure<UnifyResultSettingsOptions>(configuration.GetSection(section));

                // 是否启用规范化结果
                UnifyContext.EnabledUnifyHandler = true;

                // 添加规范化提供器
                services.AddUnifyProvider<TUnifyResultProvider>(string.Empty);

                // 添加成功规范化结果筛选器
                services.AddCollectionFilter<SucceededUnifyResultFilter>();
            }

            return services;
        }

        /// <summary>
        /// 替换默认的规范化结果
        /// </summary>
        /// <typeparam name="TUnifyResultProvider"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUnifyProvider<TUnifyResultProvider>(this IServiceCollection services)
            where TUnifyResultProvider : class, IUnifyResultProvider
        {
            return services.AddUnifyProvider<TUnifyResultProvider>(string.Empty);
        }

        /// <summary>
        /// 添加规范化提供器
        /// </summary>
        /// <typeparam name="TUnifyResultProvider"></typeparam>
        /// <param name="services"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static IServiceCollection AddUnifyProvider<TUnifyResultProvider>(this IServiceCollection services, string providerName)
            where TUnifyResultProvider : class, IUnifyResultProvider
        {
            providerName ??= string.Empty;

            var providerType = typeof(TUnifyResultProvider);

            // 添加规范化提供器
            services.TryAddSingleton(providerType, providerType);

            // 获取规范化提供器模型，不能为空
            var resultType = providerType.GetCustomAttribute<UnifyModelAttribute>().ModelType;

            // 创建规范化元数据
            var metadata = new UnifyMetadata
            {
                ProviderName = providerName,
                ProviderType = providerType,
                ResultType = resultType
            };

            // 添加或替换规范化配置
            UnifyContext.UnifyProviders.AddOrUpdate(providerName, _ => metadata, (_, _) => metadata);

            return services;
        }

        /// <summary>
        /// 添加规范化序列化配置
        /// </summary>
        /// <param name="mvcBuilder"></param>
        /// <param name="providerName"></param>
        /// <param name="serializerSettings"></param>
        /// <returns></returns>
        public static IMvcBuilder AddUnifyJsonOptions(this IMvcBuilder mvcBuilder, string providerName, object serializerSettings)
        {
            mvcBuilder.Services.AddUnifyJsonOptions(providerName, serializerSettings);

            return mvcBuilder;
        }

        /// <summary>
        /// 添加规范化序列化配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="providerName"></param>
        /// <param name="serializerSettings"></param>
        /// <returns></returns>
        public static IServiceCollection AddUnifyJsonOptions(this IServiceCollection services, string providerName, object serializerSettings)
        {
            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentNullException(nameof(providerName));

            // 添加或替换规范化序列化配置
            UnifyContext.UnifySerializerSettings.AddOrUpdate(providerName, _ => serializerSettings, (_, _) => serializerSettings);

            return services;
        }
    }
}
