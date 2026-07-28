using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Auther
{
    /// <summary>
    /// 负责NexDDD 核心模块中的 AppClaimTypes 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public static class AppClaimTypes
    {
        private static string Prefix = "yunshu:";

        public const string TenantId = "x-tenant";

        public const string UserId = "sub";

        public const string UserName = "username";

        public const string UserRole = "userrole";
    }
}
