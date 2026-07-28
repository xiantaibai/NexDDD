using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.TableExists.Abstractions;
using ShardingCore.TableExists;
using System;
using System.Reflection;
using testApi.Commands;
using testApi.EntityTableRoutes;
using testApi.MultiTenantDbContext;
using NexDDD.Provider.Snowflake;
using NexDDD.Provider.EntityFrameworkCore.Extensions;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.DynamicDataSources;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.TableCreator;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using NexDDD.Provider.EntityFrameworkCore;
using testApi.Entites;
using NexDDD.Core.CommandLocks;
using Microsoft.EntityFrameworkCore.Migrations;
using NexDDD.Core.DistributedLocks;
using testApi.Extensions;
using NexDDD.Core.Idempotency;
using NexDDD.Provider.EntityFrameworkCore.Idempotency;
using NexDDD.Core.DatabaseAccessor.Repositories;

var builder = WebApplication.CreateBuilder(args);

var efLogger = LoggerFactory.Create(builder =>
{
    builder.AddFilter((category, level) =>
        category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

SnowflakeOptions? SnowflakeOption = builder.Configuration.GetSection("SnowId").Get<SnowflakeOptions>();
builder.Services.AddSnowflake(options =>
{
    options = SnowflakeOption;
});

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());

    cfg.AddCommandLockBehavior()
    .AddIdempotentBehavior()
    .AddUnitOfWorkBehaviors();
    // .AddOpenBehavior(typeof(NewCommandUnitOfWorkBehavior<,>));
});
builder.Services.AddCommandLocks(typeof(Program).Assembly);

// 配置连接字符串
var constr = "server=114.55.176.40;port=3307;database=ShardingTest;user=api;password=Aa!@#4567899;CharSet=utf8;sslmode=none;AllowLoadLocalInfile=true";

builder.Services.AddShardingDbContext<TestDbContext>()
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

        op.UseShardingMigrationConfigure(b =>
        {
            b.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>();
        });

        op.AddDefaultDataSource("ds0", constr);
    })
     .ReplaceService<ITableEnsureManager, CusMySqlTableEnsureManager>()
     .ReplaceService<IDbContextCreator, CustomerDbContextCreator>(ServiceLifetime.Singleton)
    .AddShardingCore();

builder.Services.Replace(
                ServiceDescriptor.Singleton<IDbContextCreator, CustomerDbContextCreator>());

// 
builder.Services.AddUnitOfWork<TestDbContext>();

// 加载读 仓储
builder.Services.AddReadableRepositories<TestDbContext>(new[] { typeof(Program).Assembly });

// 
builder.Services.AddInMemoryDistributedLock();

builder.Services.AddDbIdempotency<TestDbContext>(
    new[] { typeof(Program).Assembly },
    options =>
    {
        options.DefaultExpire = TimeSpan.FromMinutes(2);
        options.CleanupInterval = TimeSpan.FromMinutes(1);
        options.CleanupBatchSize = 200;
    });

builder.Services.AddIntegrationEvent();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 
app.Services.UseAutoTryCompensateTable();
await app.Services.InitializeIdempotencyTableAsync<TestDbContext>();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/blog", async ([FromServices] IQueryRepository<Blog> queryRepository, [FromQuery] string name) =>
{
   var result=await queryRepository.FirstOrDefaultAsync(s => s.Name == name);
    return result;
})
.WithName("GetBlog")
.WithOpenApi();

app.MapPost("/blog", async ([FromServices] IMediator mediator, [FromBody] CreateBlogCommand command) =>
{
   await mediator.Send(command);
})
.WithName("CreateBlog")
.WithOpenApi();

app.MapPost("/blog2", async ([FromServices] TestDbContext dbcontext, [FromBody] CreateBlogCommand request) =>
{
    var blog = new Blog(request.name, request.code);

    using (var tran = await dbcontext.Database.BeginTransactionAsync())
    {
        var res = dbcontext.Add(blog);

        await dbcontext.SaveChangesAsync();

        await tran.CommitAsync();
    }
})
.WithName("CreateBlog2")
.WithOpenApi();

app.Run();

/// <summary>
/// 承载示例与回归验证流程中的不可变消息数据，供处理器、管线或基础设施组件传递。
/// </summary>
internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}



