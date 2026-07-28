using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Auther
{
    /// <summary>
    /// 授权用户
    /// 定义NexDDD 核心模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface IAutherUser
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        long UserId { get; }

        /// <summary>
        /// 用户名
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// 租户Id
        /// </summary>
        long? TenantId { get; }
    }
}
