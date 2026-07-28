using ExceptionDemoApi;
using Microsoft.Extensions.Options;
using NexDDD.Core.App.Extensions;
using NexDDD.Core.Exceptions.Handlers;
using NexDDD.Core.Localization.Extensions;
using NexDDD.Core.UnifyResult.Extensions;
using NexDDD.Core.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args)
    .InitNexDDD();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAppLocalization();

builder.Services.AddNexDDD(options =>
{
    //options.ConfigureFriendlyException(opt =>
    //{
    //    opt.GlobalEnabled = false;
    //});
});
//builder.Services.AddInnerDependencyInjection(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAppLocalization();
app.UseUnifyResultStatusCodes();
app.MapControllers();

app.Run();
