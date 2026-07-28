using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.TimeZone
{
    /// <summary>
    /// 提供时间与时区模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
    public static class ClockSetup
    {
        public static void AddClock(this IServiceCollection services, Action<ClockOptions>? action =null, Action<TimeZoneOptions>? timeZoneOptions = null)
        {
            if (action != null)
            {
                services.Configure(action);
            }
            if (timeZoneOptions != null)
            {
                services.Configure(timeZoneOptions);
            }

            // 检查是否已注册 IHttpContextAccessor
            if (!services.Any(d => d.ServiceType == typeof(IHttpContextAccessor)))
            {
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            }

            services.AddTransient<TimeZoneResolver>();
            services.AddTransient<IClock, Clock>();
        }
    }
}
