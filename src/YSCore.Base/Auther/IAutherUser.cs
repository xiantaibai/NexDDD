using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Auther
{
    /// <summary>
    /// 授权用户
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
