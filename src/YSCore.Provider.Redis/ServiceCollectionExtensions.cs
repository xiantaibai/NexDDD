
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis.Extensions.Protobuf;
using StackExchange.Redis.Extensions.System.Text.Json;

namespace YSCore.Provider.Redis
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisSettingsOptions(this IServiceCollection services, string section = "RedisSettings")
        {
            services.AddOptions<RedisOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    var configSection = configuration.GetSection(section);
                    if (!configSection.Exists())
                    {
                        throw new InvalidOperationException($"Configuration section '{section}' does not exist.");
                    }
                    configSection.Bind(settings);
                })
                .ValidateDataAnnotations() // 验证数据注解（如果 JWTSettingsOptions 有验证规则）
                .ValidateOnStart(); // 启动时验证配置

            return services;
        }

        public static IServiceCollection AddRedisCore(this IServiceCollection services, Action<RedisOptions> configureOptions)
        {
            var options = new RedisOptions();
            configureOptions?.Invoke(options);

            // 根据序列化类型注册 Redis 扩展
            var sv = options.SerializeType switch
            {
                SerializeTypeEnum.TextJson =>
                    services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(options.Instances),

                SerializeTypeEnum.Protobuf =>
                    services.AddStackExchangeRedisExtensions<ProtobufSerializer>(options.Instances),

                _ => throw new NotSupportedException($"不支持的序列化类型: {options.SerializeType}")
            };

            sv.AddSingleton<RedisHelper>();

            return sv;
        }
    }
}
