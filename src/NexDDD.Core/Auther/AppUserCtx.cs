using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.App;

namespace NexDDD.Core.Auther
{
    /// <summary>
    /// 负责NexDDD 核心模块中的 AppUserCtx 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public static class AppUserCtx
    {
        public static bool IsLoginin => AppHelper.User.Identity.IsAuthenticated;

        public static long TenantId => long.Parse(AppHelper.User.FindFirst(AppClaimTypes.TenantId)?.Value);

        public static string UserId => AppHelper.User.FindFirst(AppClaimTypes.UserId)?.Value;

        public static string UserName => AppHelper.User.FindFirst(AppClaimTypes.UserName)?.Value;

        public static string UserRole => AppHelper.User.FindFirst(AppClaimTypes.UserRole)?.Value;
    }
}
