using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YSCore.Base.Localization.Extensions;
using Microsoft.Extensions.DependencyInjection;
using YSCore.Base.App;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Http;
using YSCore.Base.Extensions;

namespace YSCore.Base.Localization
{
    public static class L
    {
        /// <summary>
        /// String 多语言
        /// </summary>
        public static IStringLocalizer Text => AppHelper.GetService<IStringLocalizerFactory>(AppHelper.RootServices)?.Create();

        /// <summary>
        /// Html 多语言
        /// </summary>
        public static IHtmlLocalizer Html => AppHelper.GetService<IHtmlLocalizerFactory>(AppHelper.RootServices)?.Create();

        /// <summary>
        /// String 多语言
        /// </summary>
        /// <typeparam name="T">特定类型</typeparam>
        /// <remarks><see cref="IStringLocalizer{T}"/></remarks>
        public static IStringLocalizer<T> TextOf<T>()
        {
            return AppHelper.GetService<IStringLocalizer<T>>(AppHelper.RootServices);
        }

        /// <summary>
        /// Html 多语言
        /// </summary>
        /// <typeparam name="T">特定类型</typeparam>
        /// <remarks><see cref="IHtmlLocalizer{T}"/></remarks>
        public static IHtmlLocalizer<T> HtmlOf<T>()
        {
            return AppHelper.GetService<IHtmlLocalizer<T>>(AppHelper.RootServices);
        }

        /// <summary>
        /// 设置当前选择的语言
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="immediately">是否立即对当前线程有效，设置 true 表示立即有效，默认情况下只会影响下一次请求</param>
        public static void SetCulture(string culture, bool immediately = false)
        {
            // 是否立即修改当前线程 UI 区域性
            if (immediately) SetCurrentUICulture(culture);

            var httpContext = AppHelper.HttpContext;
            if (httpContext == null) return;

            // 如果 Response 已经完成输出或者是 WebSocket 请求，则禁止写入
            if (httpContext.IsWebSocketRequest() || httpContext.Response.HasStarted) return;

            var cultureInfo = new RequestCulture(culture);

            // 修复 DateTime 问题
            Penetrates.FixedCultureDateTimeFormat(cultureInfo, AppHelper.GetOptions<LocalizationSettingsOptions>()?.DateTimeFormatCulture);

            httpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(cultureInfo),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
        }

        /// <summary>
        /// 设置当前线程 UI 区域性
        /// </summary>
        /// <param name="culture"></param>
        /// <remarks>对当前线程（代码）立即有效</remarks>
        /// <returns></returns>
        public static void SetCurrentUICulture(string culture)
        {
            // https://learn.microsoft.com/zh-cn/dotnet/api/system.globalization.cultureinfo.currentuiculture?view=net-6.0
            // 修改线程当前的 UI 区域性
            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            // 修复 DateTime 问题
            Penetrates.FixedCultureDateTimeFormat(CultureInfo.CurrentUICulture
                , AppHelper.GetOptions<LocalizationSettingsOptions>()?.DateTimeFormatCulture);
        }

        /// <summary>
        /// 获取当前选择的语言
        /// </summary>
        /// <returns></returns>
        public static RequestCulture GetSelectCulture()
        {
            var httpContext = AppHelper.HttpContext;
            if (httpContext == null) return default;

            // 获取请求特性
            var requestCulture = httpContext.Features.Get<IRequestCultureFeature>();
            return requestCulture.RequestCulture;
        }

        /// <summary>
        /// 获取系统提供的语言列表
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetCultures()
        {
            var httpContext = AppHelper.HttpContext;
            if (httpContext == null) return new Dictionary<string, string>();

            // 获取请求本地特性选项
            var locOptions = httpContext.RequestServices.GetService<IOptions<RequestLocalizationOptions>>().Value;

            // 获取语言符号和名称
            var cultureItems = locOptions.SupportedUICultures
                .ToDictionary(u => u.Name, u => u.DisplayName);

            return cultureItems;
        }

        /// <summary>
        /// 获取指定区域的翻译
        /// </summary>
        /// <param name="name"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static LocalizedString GetString(string name, string culture)
        {
            // 获取当前的区域
            var currentCulture = GetSelectCulture();

            // 临时设置区域
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            var cultureString = Text[name];

            // 还原区域
            CultureInfo.CurrentUICulture = currentCulture.Culture;
            return cultureString;
        }

        /// <summary>
        /// 获取本地配置默认语言
        /// </summary>
        /// <returns></returns>
        public static CultureInfo GetDefaultCulture()
        {
            // 获取本地多语言配置
            var localizationSettings = AppHelper.GetOptions<LocalizationSettingsOptions>();
            var defaultCulture = localizationSettings?.DefaultCulture ?? localizationSettings?.SupportedCultures.FirstOrDefault();

            return !string.IsNullOrWhiteSpace(defaultCulture)
                ? new CultureInfo(defaultCulture)
                : CultureInfo.DefaultThreadCurrentUICulture;
        }
    }
}
