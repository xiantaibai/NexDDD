using Company.Project.Domain.Products;
using Company.Project.Infrastructure.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexDDD.Provider.EntityFrameworkCore.Extensions;

namespace Company.Project.Infrastructure.DependencyInjection;

/// <summary>
/// 集中注册数据库、工作单元以及读写仓储等基础设施能力。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 将项目基础设施能力注册到依赖注入容器。
    /// </summary>
    public static IServiceCollection AddProjectInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("缺少 ConnectionStrings:Default 配置");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36)));
        });

        services.AddUnitOfWork<AppDbContext>();
        services.AddWriteRepositories(typeof(InfrastructureAssemblyMarker).Assembly);
        services.AddReadableRepositories<AppDbContext>(
            typeof(Product).Assembly,
            typeof(InfrastructureAssemblyMarker).Assembly);

        return services;
    }
}
