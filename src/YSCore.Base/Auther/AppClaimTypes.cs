using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Auther
{
    public static class AppClaimTypes
    {
        private static string Prefix = "yunshu:";

        public const string TenantId = "x-tenant";

        public const string UserId = "sub";

        public const string UserName = "username";

        public const string UserRole = "userrole";
    }
}
