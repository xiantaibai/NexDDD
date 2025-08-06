using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.TimeZone
{
    /// <summary>
    /// 时区默认配置
    /// </summary>
    public class TimeZoneOptions
    {
        /// <summary>
        /// 区域到时区的映射，例如 "zh-CN": "China Standard Time"
        /// </summary>
        public Dictionary<string, string> TimeZoneMap { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "en-US", "Eastern Standard Time" },
            { "zh-CN", "China Standard Time" },
            { "ja-JP", "Japan Standard Time" }
        };

        /// <summary>
        /// 默认时区 ID，默认为 "UTC"
        /// </summary>
        public string DefaultTimeZoneId { get; set; } = "UTC";
    }
}
