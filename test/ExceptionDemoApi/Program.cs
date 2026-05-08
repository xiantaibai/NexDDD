using ExceptionDemoApi;
using Microsoft.Extensions.Options;
using YSCore.Base.App.Extensions;
using YSCore.Base.Exceptions.Handlers;
using YSCore.Base.Localization.Extensions;
using YSCore.Base.UnifyResult.Extensions;
using YSCore.Base.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args)
    .InitYsCore();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAppLocalization();

builder.Services.AddYsCore(options =>
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
