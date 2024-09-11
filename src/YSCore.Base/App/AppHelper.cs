using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.App
{
    public static class AppHelper
    {
        /// <summary>
        /// 应用服务
        /// </summary>
        public static IServiceCollection InternalServices { get; private set; }

        /// <summary>
        /// 根服务
        /// </summary>
        public static IServiceProvider RootServices { get; private set; }

        /// <summary>
        /// 配置对象
        /// </summary>
        public static IConfiguration Configuration { get; private set; }

        /// <summary>
        /// 获取Web主机环境
        /// </summary>
        public static IWebHostEnvironment WebHostEnvironment { get; private set; }

        /// <summary>
        /// 获取泛型主机环境
        /// </summary>
        public static IHostEnvironment HostEnvironment { get; private set; }

        /// <summary>
        /// 获取请求上下文
        /// </summary>
        public static HttpContext HttpContext => CatchOrDefault(() => RootServices?.GetService<IHttpContextAccessor>()?.HttpContext);

        /// <summary>
        /// 获取请求上下文用户
        /// </summary>
        /// <remarks>只有授权访问的页面或接口才存在值，否则为 null</remarks>
        public static ClaimsPrincipal User => HttpContext?.User;


        public static void ConfigureApplication(IWebHostBuilder builder)
        {
            // 应用初始化服务
            builder.ConfigureServices((hostContext, services) =>
            {
                // 存储配置对象
                Configuration = hostContext.Configuration;

                // 存储服务提供器
                InternalServices = services;

                // 存储根服务
                // RootServices = services.BuildServiceProvider();

                // 存储环境信息
                WebHostEnvironment = hostContext.HostingEnvironment;

                HostEnvironment = hostContext.HostingEnvironment;


                // 注册 HttpContextAccessor 服务
                services.AddHttpContextAccessor();

                // 初始化应用服务
                // services.AddApp();
            });
        }

        public static void SetRootServices(IServiceProvider services)
        {
            RootServices = services;
        }

        /// <summary>
        /// 处理获取对象异常问题
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="action">获取对象委托</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>T</returns>
        private static T CatchOrDefault<T>(Func<T> action, T defaultValue = null)
            where T : class
        {
            try
            {
                return action();
            }
            catch
            {
                return defaultValue ?? null;
            }
        }
    }
}
