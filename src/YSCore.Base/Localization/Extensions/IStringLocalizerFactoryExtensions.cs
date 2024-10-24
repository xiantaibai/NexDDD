using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.App;

namespace YSCore.Base.Localization.Extensions
{
    public static class IStringLocalizerFactoryExtensions
    {
        public static IStringLocalizer Create(this IStringLocalizerFactory stringLocalizerFactory)
        {
            var localizationSettings = AppHelper.GetOptions<LocalizationSettingsOptions>();

            // 处理空配置问题
            if (!string.IsNullOrWhiteSpace(localizationSettings.LanguageFilePrefix)
                && !string.IsNullOrWhiteSpace(localizationSettings.AssemblyName))
            {
                return stringLocalizerFactory.Create(localizationSettings.LanguageFilePrefix, localizationSettings.AssemblyName);
            }

            return null;
        }

        /// <summary>
        /// 创建默认多语言工厂
        /// </summary>
        /// <param name="htmlLocalizerFactory"></param>
        /// <returns></returns>
        public static IHtmlLocalizer Create(this IHtmlLocalizerFactory htmlLocalizerFactory)
        {
            var localizationSettings = AppHelper.GetOptions<LocalizationSettingsOptions>();
            return htmlLocalizerFactory.Create(localizationSettings.LanguageFilePrefix, localizationSettings.AssemblyName);
        }
    }
}
