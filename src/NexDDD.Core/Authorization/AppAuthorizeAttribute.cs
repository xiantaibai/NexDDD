using Microsoft.AspNetCore.Authorization;
namespace NexDDD.Core.Authorization
{
    /// <summary>
    /// 负责授权模块中的 AppAuthorizeAttribute 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public sealed class AppAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="policies">多个策略</param>
        public AppAuthorizeAttribute(params string[] policies)
        {
            if (policies != null && policies.Length > 0) Policies = policies;
        }

        /// <summary>
        /// 策略
        /// </summary>
        public string[] Policies
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Policy)) return Array.Empty<string>();

                return Policy[Penetrates.AppAuthorizePrefix.Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);
            }
            internal set => Policy = $"{Penetrates.AppAuthorizePrefix}{string.Join(',', value)}";
        }
    }
}
