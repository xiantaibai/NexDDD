using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.Exceptions;
using NexDDD.Core.Exceptions.Internal;
using NexDDD.Core.UnifyResult;

namespace NexDDD.Core.Authorization
{
    /// <summary>
    /// 负责授权流程的处理与编排，协调上游请求和下游服务完成当前用例。
    /// </summary>
    public abstract class AppAuthorizeHandler : IAuthorizationHandler
    {
        /// <summary>
        /// 授权验证核心方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            // 获取 HttpContext 上下文
            var httpContext = context.GetCurrentHttpContext();

            try
            {
                await HandleAsync(context, httpContext);
            }
            catch (Exception exception)
            {
                context.Fail();

                // 处理规范化结果
                await UnifyWrapper(httpContext, exception);
            }
        }

        /// <summary>
        /// 授权验证核心方法（可重写）
        /// </summary>
        /// <param name="context"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task HandleAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
        {
            // 判断是否授权
            var isAuthenticated = context.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                // 禁止使用刷新 Token 进行单独校验
                //if (_refreshTokenClaims.All(k => context.User.Claims.Any(c => c.Type == k)))
                //{
                //    context.Fail();
                //    return;
                //}

                await AuthorizeHandleAsync(context);
            }
            // else context.GetCurrentHttpContext()?.SignoutToSwagger();    // 退出 Swagger 登录
        }

        /// <summary>
        /// 验证管道
        /// </summary>
        /// <param name="context"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// 策略验证管道
        /// </summary>
        /// <param name="context"></param>
        /// <param name="httpContext"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        public virtual Task<bool> PolicyPipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext, IAuthorizationRequirement requirement)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// 授权处理
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected async Task AuthorizeHandleAsync(AuthorizationHandlerContext context)
        {
            // 获取所有未成功验证的需求
            var pendingRequirements = context.PendingRequirements;

            // 获取 HttpContext 上下文
            var httpContext = context.GetCurrentHttpContext();

            // 调用子类管道
            var pipeline = await PipelineAsync(context, httpContext);
            if (pipeline)
            {
                // 通过授权验证
                foreach (var requirement in pendingRequirements)
                {
                    // 验证策略管道
                    var policyPipeline = await PolicyPipelineAsync(context, httpContext, requirement);
                    if (policyPipeline) context.Succeed(requirement);
                    else context.Fail();
                }
            }
            else context.Fail();
        }

        /// <summary>
        /// 处理规范化结果
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static async Task UnifyWrapper(DefaultHttpContext httpContext, Exception exception)
        {
            var exceptionMetadata = UnifyContext.GetExceptionMetadata(exception, httpContext);

            // 处理规范化结果
            if (!UnifyContext.CheckExceptionHttpContextNonUnify(httpContext, out var unifyRes))
            {
                _ = UnifyContext.CheckVaildResult(unifyRes.OnAuthorizeException(httpContext, exceptionMetadata), out var data);

                // 终止返回
                httpContext.Response.StatusCode = exceptionMetadata.StatusCode;
                await httpContext.Response.WriteAsJsonAsync(data);
            }
            else throw exception;
        }
    }
}
