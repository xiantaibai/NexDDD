using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Authorization
{
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
