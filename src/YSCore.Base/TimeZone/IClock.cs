using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.TimeZone
{
    public interface IClock
    {
        /// <summary>
        /// Gets Now.
        /// </summary>
        DateTime Now { get; }

        /// <summary>
        /// Gets kind.
        /// </summary>
        DateTimeKind Kind { get; }

        /// <summary>
        /// 如果当前时间是UTC,则返回 true.
        /// </summary>
        bool SupportsMultipleTimezone { get; }

        /// <summary>
        /// Normalizes given <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateTime">DateTime to be normalized.</param>
        /// <returns>
        /// 1.如果当前时钟为UTC,并且给定的 DateTime 为本地时间,将给定的 DateTime 转换为UTC
        /// 2.如果当前时钟是本地的,并且给定的 DateTime 是UTC,将给定的 DateTime 转换为本地时间
        /// 3.如果未指定给定的 DateTime 的 Kind,将给定的 DateTime 的 Kind设置为当前时钟的 Kind.
        /// </returns>
        DateTime Normalize(DateTime dateTime);
    }
}
