using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.TableExists.Abstractions;
using YSCore.Base.App;
using YSCore.Provider.EntityFrameworkCore.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using testApi.EntityTableRoutes;
using testApi.MultiTenantDbContext;
using DotNetCore.CAP;
using YSCore.Base.Integration;
using testApi.Events;
using YSCore.Provider.Cap;

namespace testApi.Extensions
{
    public static class DBExtension
    {
        /// <summary>
        /// 添加 测试 上下文1
        /// </summary>
        /// <param name="services"></param>
        public static void AddDBContext(this IServiceCollection services)
        {
            var constr = "server=114.55.176.40;port=3307;database=ShardingTest;user=api;password=Aa!@#4567899;CharSet=utf8;sslmode=none;AllowLoadLocalInfile=true";

            services.AddShardingDbContext<TestDbContext>()
                .UseRouteConfig(op =>
                {
                    op.AddShardingTableRoute<BlogTableRoute2>();
                })
                .UseConfig(op =>
                {
                    op.ThrowIfQueryRouteNotMatch = false;
                    op.UseShardingQuery((conn, builder) =>
                    {
                        builder.UseMySql(conn, ServerVersion.AutoDetect(conn), builder =>
                        {
                            builder.UseRelationalNulls();
                        });

                        builder.LogTo(Console.WriteLine, LogLevel.Information)
                                .EnableSensitiveDataLogging()
                                .EnableDetailedErrors();
                    });

                    op.UseShardingTransaction((connection, builder) =>
                    {
                        builder.UseMySql(connection, new MySqlServerVersion(new Version()));

                        builder.LogTo(Console.WriteLine, LogLevel.Information)
                                .EnableSensitiveDataLogging()
                                .EnableDetailedErrors();
                    });

                    op.AddDefaultDataSource("ds0", constr);
                })
                 .ReplaceService<ITableEnsureManager, CusMySqlTableEnsureManager>()
                 .ReplaceService<IDbContextCreator, CustomerDbContextCreator>(ServiceLifetime.Singleton)
                .AddShardingCore();
            // 替换
            services.Replace(
                            ServiceDescriptor.Singleton<IDbContextCreator, CustomerDbContextCreator>());

            // 事务
            services.AddUnitOfWork<TestDbContext>();
        }


        /// <summary>
        /// 添加 运营商 上下文
        /// </summary>
        /// <param name="services"></param>
        public static void AddDBContext2(this IServiceCollection services)
        {
            
        }

        public static void AddIntegrationEvent(this IServiceCollection services)
        {
            // Cap 注入
            services.AddIntegrationEventServices(typeof(CreateBlogDomainIntegrationEvent))
                .UseCap();

            //var dbop = AppHelper.GetConfig<DBOptions>("ConnectionStrings");
            //var constr = dbop.MainConnection;
            //var kafkaConfig = AppHelper.GetConfig<CapOptions>("CapSetting");
            var constr = "server=114.55.176.40;port=3307;database=ShardingTest;user=api;password=Aa!@#4567899;CharSet=utf8;sslmode=none;AllowLoadLocalInfile=true";

            services.AddCap(x =>
            {
                x.UseMySql(opt =>
                {
                    opt.ConnectionString = constr;
                    opt.TableNamePrefix = "inventory";
                });
                x.UseKafka(kafkaOption =>
                {
                    // ⭐ Kafka Broker
                    kafkaOption.Servers = "47.96.225.52:9091";

                    // ⭐ 发布确认
                    kafkaOption.MainConfig.Add("acks", "all");

                    // ⭐ 自动创建 Topic
                    kafkaOption.MainConfig.Add("allow.auto.create.topics", "true");

                    kafkaOption.CustomHeadersBuilder = (kafkaResult, sp) => new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("my.kafka.offset", kafkaResult.Offset.ToString()),
                        new KeyValuePair<string, string>("my.kafka.partition", kafkaResult.Partition.ToString())
                    };
                });

                x.UseDashboard(); //CAP Dashboard  path：  /cap
                                  //  x.Version = "iot-v1";

                x.TopicNamePrefix = "CapTest";
                x.DefaultGroupName = "testapi-service";
            });
        }
    }
}