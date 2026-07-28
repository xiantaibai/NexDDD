using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Localization
{
    /// <summary>
    /// 负责本地化模块中的 Penetrates 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    internal static class Penetrates
    {
        /// <summary>
        /// 设置请求多语言对象
        /// </summary>
        /// <param name="requestLocalization"></param>
        /// <param name="localizationSettings"></param>
        internal static void SetRequestLocalization(RequestLocalizationOptions requestLocalization, LocalizationSettingsOptions localizationSettings)
        {
            // 如果设置了默认语言，则取默认语言，否则取第一个
            requestLocalization.SetDefaultCulture(
                        string.IsNullOrWhiteSpace(localizationSettings.DefaultCulture)
                            ? (localizationSettings.SupportedCultures != null && localizationSettings.SupportedCultures.Length > 0 ?
                            localizationSettings.SupportedCultures[0] : localizationSettings.DefaultCulture)
                            : localizationSettings.DefaultCulture)
                   .AddSupportedCultures(localizationSettings.SupportedCultures)
                   .AddSupportedUICultures(localizationSettings.SupportedCultures);

            // 自动根据客户端浏览器的语言实现多语言机制
            requestLocalization.ApplyCurrentCultureToResponseHeaders = true;

            // 修复 DateTime 问题,不同 CultureInfo 获取的时间不一致，固定时间区域为特定时区，比如北京时间
            // 尽可能的使用 DateTime.UtcNow 或者 DateTimeOffset.UtNow 记录时间
            if (!string.IsNullOrWhiteSpace(localizationSettings.DateTimeFormatCulture))
            {
                var standardCulture = new CultureInfo(localizationSettings.DateTimeFormatCulture);

                // 修复默认区域语言
                FixedCultureDateTimeFormat(requestLocalization.DefaultRequestCulture, standardCulture);

                // 修复所有支持的区域语言
                foreach (var culture in requestLocalization.SupportedCultures)
                {
                    FixedCultureDateTimeFormat(culture, standardCulture);
                }

                // 修复线程区域语言
                foreach (var culture in requestLocalization.SupportedUICultures)
                {
                    FixedCultureDateTimeFormat(culture, standardCulture);
                }
            }
        }

        /// <summary>
        /// 修复多语言引起的 DateTime.Now 问题
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="targetCulture"></param>
        internal static void FixedCultureDateTimeFormat(CultureInfo culture, CultureInfo targetCulture)
        {
            culture.DateTimeFormat = targetCulture.DateTimeFormat;
        }

        /// <summary>
        /// 修复多语言引起的 DateTime.Now 问题
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="targetCulture"></param>
        internal static void FixedCultureDateTimeFormat(RequestCulture culture, CultureInfo targetCulture)
        {
            culture.Culture.DateTimeFormat = targetCulture.DateTimeFormat;
            culture.UICulture.DateTimeFormat = targetCulture.DateTimeFormat;
        }

        /// <summary>
        /// 修复多语言引起的 DateTime.Now 问题
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="targetCulture"></param>
        internal static void FixedCultureDateTimeFormat(CultureInfo culture, string targetCulture)
        {
            if (!string.IsNullOrWhiteSpace(targetCulture))
            {
                FixedCultureDateTimeFormat(culture, new CultureInfo(targetCulture));
            }
        }

        /// <summary>
        /// 修复多语言引起的 DateTime.Now 问题
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="targetCulture"></param>
        internal static void FixedCultureDateTimeFormat(RequestCulture culture, string targetCulture)
        {
            if (!string.IsNullOrWhiteSpace(targetCulture))
            {
                FixedCultureDateTimeFormat(culture, new CultureInfo(targetCulture));
            }
        }
    }
}
