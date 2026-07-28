
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Protobuf;
using StackExchange.Redis.Extensions.System.Text.Json;

namespace NexDDD.Provider.Redis
{
    /// <summary>
    /// 提供Redis 基础设施模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
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

            sv.AddSingleton<StackExchangeRedisHelper>();

            return sv;
        }

        /// <summary>
        /// 注册 Redis 服务（从配置文件读取并绑定 RedisOptions，支持不同序列化方式）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="section">配置节名称，默认 RedisSettings</param>
        public static IServiceCollection AddRedisCore(
            this IServiceCollection services,
            string section = "RedisSettings")
        {
            using var provider = services.BuildServiceProvider();
            var configuration = provider.GetRequiredService<IConfiguration>();

            // 绑定并验证配置
            var redisOptions = new RedisOptions();
            var configSection = configuration.GetSection(section);
            if (!configSection.Exists())
            {
                throw new InvalidOperationException($"Configuration section '{section}' does not exist.");
            }
            configSection.Bind(redisOptions);

            var optionsBuilder = services.AddOptions<RedisOptions>()
                .Bind(configSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // 根据序列化类型注册
            services = redisOptions.SerializeType switch
            {
                SerializeTypeEnum.TextJson =>
                    services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(redisOptions.Instances),

                SerializeTypeEnum.Protobuf =>
                    services.AddStackExchangeRedisExtensions<ProtobufSerializer>(redisOptions.Instances),

                _ => throw new NotSupportedException($"不支持的序列化类型: {redisOptions.SerializeType}")
            };

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
                var instance = opts.Instances.First();

                var config = ConfigurationOptions.Parse(instance.ConnectionString);

                return ConnectionMultiplexer.Connect(config);
            });

            // Redis 帮助类
            services.AddSingleton<StackExchangeRedisHelper>();

            return services;
        }
    }
}
