using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;


namespace YSCore.Base.Localization.Extensions
{
    public static class LocalizationApplicationBuilderExtensions
    {
        /// <summary>
        /// 配置多语言中间件拓展
        /// </summary>
        /// <param name="app"></param>
        /// <param name="customizeConfigure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseAppLocalization(this IApplicationBuilder app, Action<RequestLocalizationOptions> customizeConfigure = default)
        {
            var localizationSettings = app.ApplicationServices.GetService<IOptions<LocalizationSettingsOptions>>()?.Value;
            var requestLocalization = new RequestLocalizationOptions();

            Penetrates.SetRequestLocalization(requestLocalization, localizationSettings);

            // 使用自定义配置
            customizeConfigure?.Invoke(requestLocalization);

            // 设置多语言请求中间件
            app.UseRequestLocalization(requestLocalization);

            return app;
        }
    }
}
