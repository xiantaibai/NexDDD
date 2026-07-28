using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using NexDDD.Core.App;
using static System.Collections.Specialized.BitVector32;

namespace NexDDD.Core.Localization.Extensions
{
    /// <summary>
    /// 提供本地化模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
    public static class LocalizationServiceCollectionExtensions
    {
        /// <summary>
        /// 配置多语言服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="customizeConfigure">如果传入该参数，则使用自定义多语言机制</param>
        /// <returns></returns>
        public static IServiceCollection AddAppLocalization(this IServiceCollection services, Action<LocalizationSettingsOptions> customizeConfigure = default)
        {
            // 添加多语言配置选项
            var seciton = AppHelper.Configuration.GetSection("LocalizationSettings");
            services.Configure<LocalizationSettingsOptions>(seciton);

            var localizationSettings = seciton.Get<LocalizationSettingsOptions>();

            // 注册默认多语言服务
            if (customizeConfigure == null)
            {
                services.AddLocalization(options =>
                {
                    if (!string.IsNullOrWhiteSpace(localizationSettings.ResourcesPath))
                        options.ResourcesPath = localizationSettings.ResourcesPath;
                });
            }
            // 使用自定义
            else customizeConfigure.Invoke(localizationSettings);

            // 注册请求多语言配置选项
            services.Configure<RequestLocalizationOptions>(options =>
            {
                Penetrates.SetRequestLocalization(options, localizationSettings);
            });

            // 处理多语言在 Razor 视图中文乱码问题
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

            return services;
        }
    }
}
