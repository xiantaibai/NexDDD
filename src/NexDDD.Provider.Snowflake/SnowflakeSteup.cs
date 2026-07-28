using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yitter.IdGenerator;

namespace NexDDD.Provider.Snowflake
{
    /// <summary>
    /// 提供雪花 ID 模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
    public static class SnowflakeSteup
    {
        public static void AddSnowflake(this IServiceCollection services, Action<SnowflakeOptions> options)
        {
            var snowflakeOptions = new SnowflakeOptions();
            options.Invoke(snowflakeOptions); 
            YitIdHelper.SetIdGenerator(snowflakeOptions);
            services.AddSingleton<ISnowflakeIdGenerator, SnowflakeIdGenerator>();
        }
    }
}
