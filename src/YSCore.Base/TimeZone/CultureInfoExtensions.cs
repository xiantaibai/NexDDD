using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.TimeZone
{
    public static class CultureInfoExtensions
    {
        // 扩展方法，根据区域信息获取时区
        public static TimeZoneInfo GetTimeZone(this CultureInfo cultureInfo)
        {
            // 默认使用格林威治标准时间 (GMT) 作为时区
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Utc;

            // 根据区域信息的标识符获取时区
            // 你可以根据具体的区域信息来确定时区
            // 这里只是一个简单的示例，你需要根据实际情况来确定时区
            switch (cultureInfo.Name)
            {
                case "en-US":
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    break;
                case "zh-CN":
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
                    break;
                // 可以根据其他区域信息继续添加更多的情况
                default:
                    // 默认使用 UTC 时区
                    break;
            }

            return timeZoneInfo;
        }
    }
}
