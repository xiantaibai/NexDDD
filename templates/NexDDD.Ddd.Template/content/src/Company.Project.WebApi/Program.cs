using NexDDD.Core.App.Extensions;
using NexDDD.Core.UnifyResult.Extensions;
using Company.Project.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args)
    .InitNexDDD()
    .AddApplicationServices();

var app = builder.Build();

app.UseUnifyResultStatusCodes();
app.MapControllers();

app.Run();
