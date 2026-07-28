using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.TimeZone
{
    /// <summary>
    /// 时区转换
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// 获取当前时钟对应的时间。
        /// </summary>
        DateTime Now { get; }

        /// <summary>
        /// 获取当前时钟使用的 DateTimeKind。
        /// </summary>
        DateTimeKind Kind { get; }

        /// <summary>
        /// 如果当前时间是UTC,则返回 true.
        /// </summary>
        bool SupportsMultipleTimezone { get; }

        /// <summary>
        /// 根据当前时钟类型规范化给定的 <see cref="DateTime"/>。
        /// </summary>
        /// <param name="dateTime">需要规范化的时间。</param>
        /// <returns>
        /// 1.如果当前时钟为UTC,并且给定的 DateTime 为本地时间,将给定的 DateTime 转换为UTC
        /// 2.如果当前时钟是本地的,并且给定的 DateTime 是UTC,将给定的 DateTime 转换为本地时间
        /// 3.如果未指定给定的 DateTime 的 Kind,将给定的 DateTime 的 Kind设置为当前时钟的 Kind.
        /// </returns>
        DateTime Normalize(DateTime dateTime);
    }
}
