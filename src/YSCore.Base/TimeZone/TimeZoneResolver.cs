using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.TimeZone
{
    public class TimeZoneResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClockOptions _clockOptions;
        private readonly TimeZoneOptions _timeZoneOptions;
        private static readonly ConcurrentDictionary<string, TimeZoneInfo> _timeZoneCache = new();

        public TimeZoneResolver(IHttpContextAccessor httpContextAccessor, IOptions<ClockOptions> clockOptions, IOptions<TimeZoneOptions> timeZoneOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _clockOptions = clockOptions.Value;
            _timeZoneOptions = timeZoneOptions.Value ?? throw new ArgumentNullException(nameof(timeZoneOptions));
        }

        /// <summary>
        /// 获取时区信息，优先级：用户声明 > 请求头 > 区域信息 > 默认配置
        /// </summary>
        /// <param name="cultureInfo">区域信息（可选）</param>
        /// <returns>解析后的时区信息</returns>
        public TimeZoneInfo GetTimeZone(CultureInfo? cultureInfo = null)
        {
            // 1. 从用户声明（Claims）获取时区
            if (_httpContextAccessor?.HttpContext?.User != null)
            {
                var timeZoneClaim = _httpContextAccessor.HttpContext.User.FindFirst("TimeZone")?.Value;
                if (!string.IsNullOrWhiteSpace(timeZoneClaim))
                {
                    return GetTimeZoneInfoOrDefault(timeZoneClaim);
                }
            }

            // 2. 从请求头获取时区
            if (_httpContextAccessor?.HttpContext?.Request.Headers.TryGetValue("Time-Zone", out var timeZoneHeader) == true
                && !string.IsNullOrWhiteSpace(timeZoneHeader))
            {
                return GetTimeZoneInfoOrDefault(timeZoneHeader);
            }

            // 3. 根据区域信息获取时区
            if (cultureInfo != null)
            {
                return GetTimeZoneFromCulture(cultureInfo);
            }

            // 4. 使用默认时区（从 ClockOptions 或 UTC）
            var defaultTimeZoneId = _timeZoneOptions.DefaultTimeZoneId;
            if (string.IsNullOrWhiteSpace(defaultTimeZoneId))
            {
                return _clockOptions.Kind == DateTimeKind.Utc ? TimeZoneInfo.Utc : TimeZoneInfo.Local;
            }

            return GetTimeZoneInfoOrDefault(defaultTimeZoneId);
        }

        /// <summary>
        /// 根据区域信息获取时区
        /// </summary>
        private TimeZoneInfo GetTimeZoneFromCulture(CultureInfo cultureInfo)
        {
            // 可通过配置文件扩展映射关系
        //    var timeZoneMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        //{
        //    { "en-US", "Eastern Standard Time" },
        //    { "zh-CN", "China Standard Time" },
        //    { "ja-JP", "Japan Standard Time" },
        //    // 更多区域与时区的映射
        //};
            var timeZoneMap = _timeZoneOptions.TimeZoneMap ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var timeZoneId = timeZoneMap.TryGetValue(cultureInfo.Name, out var id) ? id : "UTC";
            return GetTimeZoneInfoOrDefault(timeZoneId);
        }

        /// <summary>
        /// 获取 TimeZoneInfo，带缓存和异常处理
        /// </summary>
        private TimeZoneInfo GetTimeZoneInfoOrDefault(string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
            {
                return TimeZoneInfo.Utc;
            }

            // 检查缓存
            if (_timeZoneCache.TryGetValue(timeZoneId, out var cachedTimeZone))
            {
                return cachedTimeZone;
            }

            try
            {
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                _timeZoneCache.TryAdd(timeZoneId, timeZoneInfo);
                return timeZoneInfo;
            }
            catch (TimeZoneNotFoundException)
            {
                // 无效时区 ID，返回默认 UTC
                return TimeZoneInfo.Utc;
            }
            catch (InvalidTimeZoneException)
            {
                // 时区数据损坏，返回默认 UTC
                return TimeZoneInfo.Utc;
            }
        }
    }
}
