using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.App.Options;
using YSCore.Base.DataValidation.Extensions;
using YSCore.Base.Exceptions.Extensions;
using YSCore.Base.UnifyResult.Extensions;

namespace YSCore.Base.App.Extensions
{
    public static class AppServiceCollectionExtensions
    {
        /// <summary>
        /// 初始化 配置
        /// </summary>
        /// <param name="webApplicationBuilder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder InitYsCore(this WebApplicationBuilder webApplicationBuilder)
        {
            // 加载配置
            AppHelper.ConfigureApplication(webApplicationBuilder.WebHost);

            return webApplicationBuilder;
        }


        public static IServiceCollection AddYsCore(this IServiceCollection services, Action<AddYsCoreOptions> configure = null)
        {

            // 载入服务配置选项
            var configureOptions = new AddYsCoreOptions();
            configure?.Invoke(configureOptions);

            services.AddDataValidation(AddYsCoreOptions.DataValidationConfigure)
                    .AddFriendlyException(AddYsCoreOptions.FriendlyExceptionConfigure)
                    .AddUnifyResult();
            return services;
        }
    }
}
