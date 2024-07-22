using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.TimeZone
{
    public static class ClockSetup
    {
        public static void AddClock(this IServiceCollection services, Action<ClockOptions>? action =null)
        {
            if (action != null)
            {
                services.Configure(action);
            }
            services.AddTransient<IClock, Clock>();
        }
    }
}
