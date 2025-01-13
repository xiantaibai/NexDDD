using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.App;

namespace YSCore.Base.Auther
{
    public static class AppUserCtx
    {
        public static bool IsLoginin => AppHelper.User.Identity.IsAuthenticated;

        public static long TenantId => long.Parse(AppHelper.User.FindFirst(AppClaimTypes.TenantId)?.Value);

        public static string UserId => AppHelper.User.FindFirst(AppClaimTypes.UserId)?.Value;

        public static string UserName => AppHelper.User.FindFirst(AppClaimTypes.UserName)?.Value;

        public static string UserRole => AppHelper.User.FindFirst(AppClaimTypes.UserRole)?.Value;
    }
}
