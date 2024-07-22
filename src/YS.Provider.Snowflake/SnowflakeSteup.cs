using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yitter.IdGenerator;

namespace YS.Provider.Snowflake
{
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
