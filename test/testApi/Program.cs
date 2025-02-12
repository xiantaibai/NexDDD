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
using YS.Provider.Snowflake;
using YSCore.Provider.EntityFrameworkCore.Extensions;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.DynamicDataSources;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.TableCreator;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using YSCore.Provider.EntityFrameworkCore;
using testApi.Entites;

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

    cfg.AddOpenBehavior(typeof(NewCommandUnitOfWorkBehavior<,>));
    //cfg.AddUnitOfWorkBehaviors();
    //cfg.AddDataValidationBehavior()
    //.AddUnitOfWorkBehaviors();
});

// 配置连接字符串
var constr = "server=114.55.176.40;port=3307;database=ShardingTest;user=api;password=Aa!@#4567899;CharSet=utf8;sslmode=none;AllowLoadLocalInfile=true";

builder.Services.AddShardingDbContext<TestDbContext>()
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
builder.Services.Replace(
                ServiceDescriptor.Singleton<IDbContextCreator, CustomerDbContextCreator>());

// 事务
builder.Services.AddUnitOfWork<TestDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 非必须  启动检查缺少的表并且创建
app.Services.UseAutoTryCompensateTable();


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

app.MapPost("/blog", ([FromServices] IMediator mediator, [FromBody] CreateBlogCommand command) =>
{
    mediator.Send(command);
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


using (var serviceScope = app.Services.CreateScope())
{
    var defaultDbContext = serviceScope.ServiceProvider.GetService<TestDbContext>();

    using (var tran = await defaultDbContext.Database.BeginTransactionAsync())
    {
        var name = Guid.NewGuid().ToString("n");
        var code = "xxx1";
        var blog = new Blog(name, code);

        await defaultDbContext.AddAsync(blog);
        await defaultDbContext.SaveChangesAsync();

        await tran.CommitAsync();
    }
}

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
