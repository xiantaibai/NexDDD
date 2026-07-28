using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Authorization
{
    /// <summary>
    /// 负责授权模块中的 AppAuthorizeRequirement 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public sealed class AppAuthorizeRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="policies"></param>
        public AppAuthorizeRequirement(params string[] policies)
        {
            Policies = policies;
        }

        /// <summary>
        /// 策略
        /// </summary>
        public string[] Policies { get; private set; }
    }
}
