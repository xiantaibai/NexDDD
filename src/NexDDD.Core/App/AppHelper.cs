using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.App.Startups;
using NexDDD.Core.Authorization;

namespace NexDDD.Core.App
{
    /// <summary>
    /// 负责NexDDD 核心模块中的 AppHelper 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
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

        /// <summary>
        /// 未托管的对象集合
        /// </summary>
        public static readonly ConcurrentBag<IDisposable> UnmanagedObjects;


        static AppHelper()
        {
            // 未托管的对象
            UnmanagedObjects = new ConcurrentBag<IDisposable>();
        }

        public static void ConfigureApplication(IWebHostBuilder builder)
        {
            // 应用初始化服务
            builder.ConfigureServices((hostContext, services) =>
            {
                // 存储配置对象
                Configuration = hostContext.Configuration;

                // 存储服务提供器
                InternalServices = services;


                // 存储环境信息
                WebHostEnvironment = hostContext.HostingEnvironment;

                HostEnvironment = hostContext.HostingEnvironment;

                // 存储根服务
                services.AddHostedService<GenericHostLifetimeEventsHostedService>();

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

        /// <summary>
        /// 解析服务提供器
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static IServiceProvider GetServiceProvider(Type serviceType)
        {
            // 处理控制台应用程序
            if (HostEnvironment == default) return RootServices;

            // 第一选择，判断是否是单例注册且单例服务不为空，如果是直接返回根服务提供器
            if (RootServices != null && InternalServices.Where(u => u.ServiceType == (serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : serviceType))
                                                                    .Any(u => u.Lifetime == ServiceLifetime.Singleton)) return RootServices;

            // 第二选择是获取 HttpContext 对象的 RequestServices
            var httpContext = HttpContext;
            if (httpContext?.RequestServices != null) return httpContext.RequestServices;
            // 第三选择，创建新的作用域并返回服务提供器
            else if (RootServices != null)
            {
                var scoped = RootServices.CreateScope();
                UnmanagedObjects.Add(scoped);
                return scoped.ServiceProvider;
            }
            // 第四选择，构建新的服务对象（性能最差）
            else
            {
                var serviceProvider = InternalServices.BuildServiceProvider();
                UnmanagedObjects.Add(serviceProvider);
                return serviceProvider;
            }
        }

        /// <summary>
        /// 获取请求生存周期的服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static TService GetService<TService>(IServiceProvider serviceProvider = default)
            where TService : class
        {
            return GetService(typeof(TService), serviceProvider) as TService;
        }

        /// <summary>
        /// 获取请求生存周期的服务
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static object GetService(Type type, IServiceProvider serviceProvider = default)
        {
            return (serviceProvider ?? GetServiceProvider(type)).GetService(type);
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="TOptions">强类型选项类</typeparam>
        /// <param name="path">配置中对应的Key</param>
        /// <param name="loadPostConfigure"></param>
        /// <returns>TOptions</returns>
        public static TOptions GetConfig<TOptions>(string path, bool loadPostConfigure = false)
        {
            var options = Configuration.GetSection(path).Get<TOptions>();

            // 加载默认选项配置
            //if (loadPostConfigure && typeof(IConfigurableOptions).IsAssignableFrom(typeof(TOptions)))
            //{
            //    var postConfigure = typeof(TOptions).GetMethod("PostConfigure");
            //    if (postConfigure != null)
            //    {
            //        options ??= Activator.CreateInstance<TOptions>();
            //        postConfigure.Invoke(options, new object[] { options, Configuration });
            //    }
            //}

            return options;
        }


        /// <summary>
        /// 获取选项
        /// </summary>
        /// <typeparam name="TOptions">强类型选项类</typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns>TOptions</returns>
        public static TOptions GetOptions<TOptions>(IServiceProvider serviceProvider = default)
            where TOptions : class, new()
        {
            //return Penetrates.GetOptionsOnStarting<TOptions>()
            //    ?? GetService<IOptions<TOptions>>(serviceProvider ?? RootServices)?.Value;
            return GetService<IOptions<TOptions>>(serviceProvider ?? RootServices)?.Value;
        }


        /// <summary>
        /// GC 回收默认间隔
        /// </summary>
        private const int GC_COLLECT_INTERVAL_SECONDS = 5;

        /// <summary>
        /// 记录最近 GC 回收时间
        /// </summary>
        private static DateTime? LastGCCollectTime { get; set; }

        /// <summary>
        /// 释放所有未托管的对象
        /// </summary>
        public static void DisposeUnmanagedObjects()
        {
            foreach (var dsp in UnmanagedObjects)
            {
                try
                {
                    dsp?.Dispose();
                }
                finally { }
            }

            // 强制手动回收 GC 内存
            if (UnmanagedObjects.Any())
            {
                var nowTime = DateTime.UtcNow;
                if ((LastGCCollectTime == null || (nowTime - LastGCCollectTime.Value).TotalSeconds > GC_COLLECT_INTERVAL_SECONDS))
                {
                    LastGCCollectTime = nowTime;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            UnmanagedObjects.Clear();
        }
    }
}
