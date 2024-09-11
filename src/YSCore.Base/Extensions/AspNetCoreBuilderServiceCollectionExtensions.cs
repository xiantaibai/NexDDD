using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.App;

namespace YSCore.Base.Extensions
{
    public static class AspNetCoreBuilderServiceCollectionExtensions
    {
        /// <summary>
        /// 注册 Mvc 过滤器
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="mvcBuilder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IMvcBuilder AddCollectionFilter<TFilter>(this IMvcBuilder mvcBuilder, Action<MvcOptions> configure = default)
            where TFilter : IFilterMetadata
        {
            mvcBuilder.Services.AddCollectionFilter<TFilter>(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// 注册 Mvc 过滤器
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddCollectionFilter<TFilter>(this IServiceCollection services, Action<MvcOptions> configure = default)
            where TFilter : IFilterMetadata
        {
            // 非 Web 环境跳过注册
            if (AppHelper.WebHostEnvironment == default) return services;

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<TFilter>();

                // 其他额外配置
                configure?.Invoke(options);
            });

            return services;
        }

        /// <summary>
        /// 注册 Mvc 过滤器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="filter"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddCollectionFilter(this IServiceCollection services, IFilterMetadata filter, Action<MvcOptions> configure = default)
        {
            // 非 Web 环境跳过注册
            if (AppHelper.WebHostEnvironment == default) return services;

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(filter);

                // 其他额外配置
                configure?.Invoke(options);
            });

            return services;
        }
    }
}
