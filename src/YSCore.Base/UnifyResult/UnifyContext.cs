using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reflection;
using YSCore.Base.App;
using YSCore.Base.Diagnostics;
using YSCore.Base.Exceptions;
using YSCore.Base.Exceptions.Internal;
using YSCore.Base.Extensions;
using YSCore.Base.UnifyResult.Attributes;
using YSCore.Base.UnifyResult.Internal;
using YSCore.Base.UnifyResult.Options;
using YSCore.Base.UnifyResult.Providers;

namespace YSCore.Base.UnifyResult
{
    /// <summary>
    /// 统一返回上下文。
    /// </summary>
    public static class UnifyContext
    {
        /// <summary>
        /// 是否启用统一返回处理。
        /// </summary>
        internal static bool EnabledUnifyHandler = false;

        /// <summary>
        /// 是否启用状态码中间件。
        /// </summary>
        internal static bool EnabledStatusCodesMiddleware = false;

        /// <summary>
        /// HttpContext.Items 中统一附加数据的键名。
        /// </summary>
        internal static string UnifyResultExtrasKey = "UNIFY_RESULT_EXTRAS";

        /// <summary>
        /// 已注册的统一返回提供器。
        /// </summary>
        internal static ConcurrentDictionary<string, UnifyMetadata> UnifyProviders = new();

        /// <summary>
        /// 已注册的统一返回序列化配置。
        /// </summary>
        internal static ConcurrentDictionary<string, object> UnifySerializerSettings = new();

        /// <summary>
        /// 根据 MVC ActionContext 构建异常元数据。
        /// </summary>
        public static ExceptionMetadata GetExceptionMetadata(ActionContext context)
        {
            var exception = context is ExceptionContext exContext
                ? exContext.Exception
                : context is ActionExecutedContext edContext
                    ? edContext.Exception
                    : default;

            return GetExceptionMetadata(exception, context.HttpContext);
        }

        /// <summary>
        /// 根据异常和 HttpContext 构建异常元数据。
        /// </summary>
        public static ExceptionMetadata GetExceptionMetadata(Exception exception, HttpContext httpContext = null)
        {
            var friendlyException = exception as AppFriendlyException;
            var traceId = YSCoreTraceContext.GetTraceId(httpContext);
            var error = ExceptionHelper.BuildErrorInfo(exception, traceId);
            object errors = error?.Message;

            if (friendlyException?.ValidationException == true)
            {
                errors = friendlyException.ErrorMessage;
            }

            return new ExceptionMetadata
            {
                StatusCode = error?.StatusCode ?? StatusCodes.Status500InternalServerError,
                ErrorCode = error?.Code,
                OriginErrorCode = error?.OriginCode,
                Errors = errors,
                Data = friendlyException?.Data,
                Exception = exception,
                Error = error,
                PublicError = ExceptionHelper.BuildPublicError(error),
                TraceId = traceId
            };
        }

        /// <summary>
        /// 在当前请求中写入统一附加数据。
        /// </summary>
        public static void Fill(object extras)
        {
            var items = AppHelper.HttpContext?.Items;
            if (items == null)
            {
                return;
            }

            if (items.ContainsKey(UnifyResultExtrasKey)) items.Remove(UnifyResultExtrasKey);
            items.Add(UnifyResultExtrasKey, extras);
        }

        /// <summary>
        /// 从当前请求中读取统一附加数据。
        /// </summary>
        public static object Take()
        {
            object extras = null;
            AppHelper.HttpContext?.Items?.TryGetValue(UnifyResultExtrasKey, out extras);
            return extras;
        }

        /// <summary>
        /// 按统一返回配置设置真实的 HTTP 响应状态码。
        /// </summary>
        public static void SetResponseStatusCodes(HttpContext context, int statusCode, UnifyResultSettingsOptions unifyResultSettings)
        {
            if (unifyResultSettings == null) return;

            if (unifyResultSettings.AdaptStatusCodes != null && unifyResultSettings.AdaptStatusCodes.Length > 0)
            {
                var adaptStatusCode = unifyResultSettings.AdaptStatusCodes.FirstOrDefault(u => u[0] == statusCode);
                if (adaptStatusCode != null && adaptStatusCode.Length > 0 && adaptStatusCode[0] > 0)
                {
                    context.Response.StatusCode = adaptStatusCode[1];
                    return;
                }
            }

            if (unifyResultSettings.Return200StatusCodes == null)
            {
                context.Response.StatusCode = 200;
            }
            else if (unifyResultSettings.Return200StatusCodes.Contains(statusCode))
            {
                context.Response.StatusCode = 200;
            }
        }

        /// <summary>
        /// 获取当前 MVC 过滤器上下文对应的序列化配置。
        /// </summary>
        public static object GetSerializerSettings(FilterContext context)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor) return null;

            var unifySerializerSettingAttribute = actionDescriptor.MethodInfo.GetFoundAttribute<UnifySerializerSettingAttribute>(true);
            if (unifySerializerSettingAttribute == null || string.IsNullOrWhiteSpace(unifySerializerSettingAttribute.Name)) return null;

            var succeed = UnifySerializerSettings.TryGetValue(unifySerializerSettingAttribute.Name, out var serializerSettings);
            return succeed ? serializerSettings : null;
        }

        /// <summary>
        /// 按提供器名称获取序列化配置。
        /// </summary>
        public static object GetSerializerSettings(string name)
        {
            var succeed = UnifySerializerSettings.TryGetValue(name, out var serializerSettings);
            return succeed ? serializerSettings : null;
        }

        /// <summary>
        /// 获取当前终结点上下文对应的序列化配置。
        /// </summary>
        public static object GetSerializerSettings(DefaultHttpContext context)
        {
            var endpointFeature = context?.Features?.Get<IEndpointFeature>();
            if (endpointFeature == null) return null;

            var unifySerializerSettingAttribute = context.GetMetadata<UnifySerializerSettingAttribute>()
                ?? endpointFeature.Endpoint?.Metadata?.GetMetadata<UnifySerializerSettingAttribute>();
            if (unifySerializerSettingAttribute == null || string.IsNullOrWhiteSpace(unifySerializerSettingAttribute.Name)) return null;

            var succeed = UnifySerializerSettings.TryGetValue(unifySerializerSettingAttribute.Name, out var serializerSettings);
            return succeed ? serializerSettings : null;
        }

        /// <summary>
        /// 检查成功结果是否需要跳过统一返回包装。
        /// </summary>
        internal static bool CheckSucceededNonUnify(MethodInfo method, out IUnifyResultProvider unifyResult, bool isWebRequest = true)
        {
            var unityMetadata = GetMethodUnityMetadata(method);

            var isSkip = !EnabledUnifyHandler
                  || method.GetRealReturnType().HasImplementedRawGeneric(unityMetadata.ResultType)
                  || method.CustomAttributes.Any(x => typeof(NonUnifyAttribute).IsAssignableFrom(x.AttributeType) || typeof(ProducesResponseTypeAttribute).IsAssignableFrom(x.AttributeType) || typeof(IApiResponseMetadataProvider).IsAssignableFrom(x.AttributeType))
                  || method.ReflectedType.IsDefined(typeof(NonUnifyAttribute), true)
                  || method.DeclaringType.Assembly.GetName().Name.StartsWith("Microsoft.AspNetCore.OData");

            if (!isWebRequest)
            {
                unifyResult = null;
                return isSkip;
            }

            unifyResult = isSkip ? null : AppHelper.RootServices.GetService(unityMetadata.ProviderType) as IUnifyResultProvider;
            return unifyResult == null || isSkip;
        }

        /// <summary>
        /// 检查失败结果是否需要跳过统一返回包装。
        /// </summary>
        internal static bool CheckFailedNonUnify(MethodInfo method, out IUnifyResultProvider unifyResult)
        {
            var unityMetadata = GetMethodUnityMetadata(method);

            var isSkip = !EnabledUnifyHandler
                    || method.CustomAttributes.Any(x => typeof(NonUnifyAttribute).IsAssignableFrom(x.AttributeType))
                    || (
                            !method.CustomAttributes.Any(x => typeof(ProducesResponseTypeAttribute).IsAssignableFrom(x.AttributeType) || typeof(IApiResponseMetadataProvider).IsAssignableFrom(x.AttributeType))
                            && method.ReflectedType.IsDefined(typeof(NonUnifyAttribute), true)
                        )
                    || method.DeclaringType.Assembly.GetName().Name.StartsWith("Microsoft.AspNetCore.OData");

            unifyResult = isSkip ? null : AppHelper.RootServices.GetService(unityMetadata.ProviderType) as IUnifyResultProvider;
            return unifyResult == null || isSkip;
        }

        /// <summary>
        /// 检查短路 HTTP 异常响应是否需要跳过统一返回包装。
        /// </summary>
        internal static bool CheckExceptionHttpContextNonUnify(HttpContext context, out IUnifyResultProvider unifyResult)
        {
            var endpointFeature = context.Features?.Get<IEndpointFeature>();
            if (endpointFeature == null) return (unifyResult = null) == null;

            var isSkip = !EnabledUnifyHandler
                    || context.GetMetadata<NonUnifyAttribute>() != null
                    || endpointFeature.Endpoint?.Metadata?.GetMetadata<NonUnifyAttribute>() != null
                    || context.Request.Headers["accept"].ToString().Contains("odata.metadata=", StringComparison.OrdinalIgnoreCase)
                    || context.Request.Headers["accept"].ToString().Contains("odata.streaming=", StringComparison.OrdinalIgnoreCase)
                    || ResponseContentTypesOfNonUnify.Any(u => context.Response.Headers["content-type"].ToString().Contains(u, StringComparison.OrdinalIgnoreCase));

            if (isSkip)
            {
                unifyResult = null;
            }
            else
            {
                var unifyProviderAttribute = endpointFeature.Endpoint?.Metadata?.GetMetadata<UnifyProviderAttribute>();
                UnifyProviders.TryGetValue(unifyProviderAttribute?.Name ?? string.Empty, out var unityMetadata);

                unifyResult = unityMetadata?.ProviderType == null
                    ? null
                    : context.RequestServices.GetService(unityMetadata.ProviderType) as IUnifyResultProvider;
            }

            return unifyResult == null || isSkip;
        }

        /// <summary>
        /// 检查是否支持 MVC 控制器统一返回处理。
        /// </summary>
        internal static bool CheckSupportMvcController(HttpContext httpContext, ControllerActionDescriptor actionDescriptor, out UnifyResultSettingsOptions unifyResultSettings)
        {
            unifyResultSettings = httpContext.RequestServices.GetService<IOptions<UnifyResultSettingsOptions>>()?.Value;

            if (unifyResultSettings?.SupportMvcController == false && typeof(Controller).IsAssignableFrom(actionDescriptor.ControllerTypeInfo)) return false;

            return true;
        }

        /// <summary>
        /// 需要跳过统一返回处理的响应内容类型。
        /// </summary>
        internal static string[] ResponseContentTypesOfNonUnify = new[]
        {
            "text/event-stream",
            "application/pdf",
            "application/octet-stream",
            "image/"
        };

        /// <summary>
        /// 检查当前 HttpContext 是否需要跳过统一返回处理。
        /// </summary>
        internal static bool CheckHttpContextNonUnify(HttpContext httpContext)
        {
            var contentType = httpContext.Response.Headers["content-type"].ToString();
            if (ResponseContentTypesOfNonUnify.Any(u => contentType.Contains(u, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查当前结果是否可以作为统一数据结果处理。
        /// </summary>
        internal static bool CheckVaildResult(IActionResult result, out object data)
        {
            data = default;

            var isDataResult = result switch
            {
                ViewResult => false,
                PartialViewResult => false,
                FileResult => false,
                ChallengeResult => false,
                SignInResult => false,
                SignOutResult => false,
                RedirectToPageResult => false,
                RedirectToRouteResult => false,
                RedirectResult => false,
                RedirectToActionResult => false,
                LocalRedirectResult => false,
                ForbidResult => false,
                ViewComponentResult => false,
                PageResult => false,
                NotFoundResult => false,
                NotFoundObjectResult => false,
                _ => true,
            };

            if (isDataResult)
            {
                data = result switch
                {
                    ContentResult content => content.Content,
                    ObjectResult obj => obj.Value,
                    JsonResult json => json.Value,
                    _ => null,
                };
            }

            return isDataResult;
        }

        /// <summary>
        /// 获取方法对应的统一返回元数据。
        /// </summary>
        /// <remarks>
        /// 如果后续有需要，这里可以继续做更激进的缓存优化。
        /// </remarks>
        internal static UnifyMetadata GetMethodUnityMetadata(MethodInfo method)
        {
            if (method == default) return default;

            var unityProviderAttribute = method.GetFoundAttribute<UnifyProviderAttribute>(true);

            var isExists = UnifyProviders.TryGetValue(unityProviderAttribute?.Name ?? string.Empty, out var metadata);
            if (!isExists)
            {
                UnifyProviders.TryGetValue(string.Empty, out metadata);
            }

            return metadata;
        }
    }
}
