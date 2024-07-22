using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Provider.Snowflake
{
    public interface ISnowflakeIdGenerator
    {
        /// <summary>
        /// 获取下一个ID
        /// </summary>
        /// <returns></returns>
        long NextId();
    }
}
