using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.App.Options;
using NexDDD.Core.DataValidation.Extensions;
using NexDDD.Core.Exceptions.Extensions;
using NexDDD.Core.UnifyResult.Extensions;

namespace NexDDD.Core.App.Extensions
{
    /// <summary>
    /// 提供NexDDD 核心模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
    public static class AppServiceCollectionExtensions
    {
        /// <summary>
        /// 初始化 配置
        /// </summary>
        /// <param name="webApplicationBuilder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder InitNexDDD(this WebApplicationBuilder webApplicationBuilder)
        {
            // 加载配置
            AppHelper.ConfigureApplication(webApplicationBuilder.WebHost);

            return webApplicationBuilder;
        }


        public static IServiceCollection AddNexDDD(this IServiceCollection services, Action<AddNexDDDOptions> configure = null)
        {

            // 载入服务配置选项
            var configureOptions = new AddNexDDDOptions();
            configure?.Invoke(configureOptions);

            services.AddDataValidation(AddNexDDDOptions.DataValidationConfigure)
                    .AddFriendlyException(AddNexDDDOptions.FriendlyExceptionConfigure)
                    .AddUnifyResult();
            return services;
        }
    }
}
