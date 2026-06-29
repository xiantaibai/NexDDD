using YSCore.Base.App.Extensions;
using YSCore.Base.UnifyResult.Extensions;
using Company.Project.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args)
    .InitYsCore()
    .AddApplicationServices();

var app = builder.Build();

app.UseUnifyResultStatusCodes();
app.MapControllers();

app.Run();
