using Microsoft.AspNetCore.Http;
using System;
using NexDDD.Core.Context;

namespace NexDDD.Core.AutherUserContext
{
    /// <summary>
    /// 授权用户 上下文构建工厂
    /// </summary>
    public class AutherUserContextFactory : IContextFactory<AutherUserContext>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AutherUserContextFactory(IHttpContextAccessor httpContextAccessor = null)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public AutherUserContext CreateDefault()
        {
            var context = new AutherUserContext();
            if (_httpContextAccessor != null)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    context.UserId = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                    context.UserName = user.FindFirst("userName")?.Value;
                    context.TenantId = user.FindFirst("tenantId")?.Value ?? "default";
                    context.OrgId = user.FindFirst("orgId")?.Value;
                    context.OrgName = user.FindFirst("orgName")?.Value;

                    context.GroupId = user.FindFirst("groupId")?.Value;
                    context.LoginFrom = user.FindFirst("loginFrom")?.Value;
                    context.LoginSite = user.FindFirst("loginSite")?.Value;
                }
            }
            return context.IsAuth ? null : context;
        }
    }
}
