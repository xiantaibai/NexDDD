using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.TimeZone
{
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
