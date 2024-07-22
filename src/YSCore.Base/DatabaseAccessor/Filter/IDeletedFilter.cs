using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.DatabaseAccessor.Filter
{
    /// <summary>
    /// 假删除接口过滤器
    /// </summary>
    public interface IDeletedFilter
    {
        /// <summary>
        /// 软删除
        /// </summary>
        bool IsDelete { get; set; }
    }
}
