using System;

namespace NexDDD.Core.AutherUserContext
{
    /// <summary>
    /// 授权用户上下文
    /// 负责请求上下文模块中的 AutherUserContext 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    public class AutherUserContext
    {
        public string UserId { get; set; }

        public string TenantId { get; set; }

        public string OrgId { get; set; }

        public string UserName { get; set; }

        public string OrgName { get; set; }

        public string GroupId { get; set; }

        public string LoginSite { get; set; }

        public string LoginFrom { get; set; }

        public string[] Roles { get; set; }

        public bool IsAuth => string.IsNullOrWhiteSpace(UserId);

        public AutherUserContext()
        {
            UserId = string.Empty;
            TenantId = string.Empty;
            Roles = Array.Empty<string>();
        }
    }
}
