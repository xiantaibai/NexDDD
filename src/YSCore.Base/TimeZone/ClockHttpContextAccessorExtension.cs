using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.TimeZone
{
    public static class ClockHttpContextAccessorExtension
    {
        public static TimeZoneInfo GetTimeZone(this IHttpContextAccessor httpContextAccessor)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Utc;

            if (httpContextAccessor != null && httpContextAccessor.HttpContext != null)
            {
                string? TimeZone = httpContextAccessor.HttpContext?.User.FindFirst("TimeZone")?.Value;
                if (!string.IsNullOrWhiteSpace(TimeZone))
                {
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
                    return timeZoneInfo;
                }

                TimeZone = httpContextAccessor.HttpContext?.Request.Headers.FirstOrDefault(s => s.Key == "Time-Zone").Value;
                if (!string.IsNullOrWhiteSpace(TimeZone))
                {
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
                    return timeZoneInfo;
                }
                var timeZone = httpContextAccessor.HttpContext?.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture.GetTimeZone();
                return timeZone ?? timeZoneInfo;
            }

            return timeZoneInfo;
        }
    }
}
