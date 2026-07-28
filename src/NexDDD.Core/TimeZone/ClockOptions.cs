using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.TimeZone
{
    /// <summary>
    /// 承载时间与时区模块的运行配置，供依赖注入注册过程与运行时组件读取。
    /// </summary>
    public class ClockOptions
    {
        /// <summary>
        /// Default: <see cref="DateTimeKind.Utc"/>
        /// </summary>
        public DateTimeKind Kind { get; set; }

        public ClockOptions()
        {
            Kind = DateTimeKind.Utc;
        }
    }
}
