using Company.Project.Application;
using Company.Project.Infrastructure.DependencyInjection;
using MediatR;
using NexDDD.Provider.Snowflake;
using NexDDD.Core.App.Extensions;
using NexDDD.Core.CommandLocks;
using NexDDD.Core.DataValidation.Extensions;
using NexDDD.Core.DependencyInjection.Extensions;
using NexDDD.Core.DistributedLocks;
using NexDDD.Core.TimeZone;
using NexDDD.Provider.EntityFrameworkCore.Extensions;

namespace Company.Project.WebApi.Extensions;

/// <summary>
/// 集中组合 WebApi 启动所需的应用层、框架和基础设施默认服务。
/// 负责NexDDD 核心模块中的 ServiceCollectionExtensions 能力，并限定其与调用方及基础设施组件的协作边界。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册控制器、NexDDD 基础能力、时钟、命令查询处理器、应用服务和项目基础设施。
    /// </summary>
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddControllers();
        services.AddNexDDD();
        services.AddClock();

        services.AddSnowflake(options =>
        {
            builder.Configuration
                .GetSection("Snowflake")
                .Bind(options);

            options.WorkerPrefix = builder.Environment.ApplicationName;
        });

        services.AddCommands();
        services.AddServiceAndQueries();
        services.AddProjectInfrastructure(builder.Configuration);

        return builder;
    }

    /// <summary>
    /// 扫描应用层 Command、Query 和领域事件处理器，并注册验证、命令锁和 UnitOfWork 管线。
    /// </summary>
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;

        services.AddMediatR(mediatR =>
        {
            mediatR.RegisterServicesFromAssembly(applicationAssembly);

            mediatR.AddCommandLockBehavior()
                .AddDataValidationBehavior()
                .AddUnitOfWorkBehaviors();
        });

        services.AddCommandLocks(applicationAssembly);
        services.AddInMemoryDistributedLock();

        return services;
    }

    /// <summary>
    /// 扫描应用层中通过 ITransient、IScoped 或 ISingleton 标记的服务与查询组件。
    /// </summary>
    public static IServiceCollection AddServiceAndQueries(this IServiceCollection services)
    {
        services.AddInnerDependencyInjection(typeof(ApplicationAssemblyMarker).Assembly);
        return services;
    }
}
