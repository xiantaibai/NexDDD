using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using YSCore.Base.DataValidation.Internal;
using YSCore.Base.Exceptions.Internal;
using YSCore.Base.UnifyResult.Attributes;
using YSCore.Base.UnifyResult.Internal;
using YSCore.Base.UnifyResult.Options;

namespace YSCore.Base.UnifyResult.Providers
{
    [UnifyModel(typeof(AppRESTfulResult<>))]
    public class RESTfulResultProvider : IUnifyResultProvider
    {

        /// <summary>
        /// JWT 授权异常返回值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public IActionResult OnAuthorizeException(DefaultHttpContext context, ExceptionMetadata metadata)
        {
            return new JsonResult(RESTfulResult(metadata.StatusCode, data: metadata.Data, errors: metadata.Errors)
                , UnifyContext.GetSerializerSettings(context));
        }

        /// <summary>
        /// 异常返回值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public IActionResult OnException(ExceptionContext context, ExceptionMetadata metadata)
        {
            return new JsonResult(RESTfulResult(metadata.StatusCode, data: metadata.Data, errors: metadata.Errors)
                , UnifyContext.GetSerializerSettings(context));
        }

        public async Task OnResponseStatusCodes(HttpContext context, int statusCode, UnifyResultSettingsOptions unifyResultSettings = null)
        {
            // 设置响应状态码
            UnifyContext.SetResponseStatusCodes(context, statusCode, unifyResultSettings);

            switch (statusCode)
            {
                // 处理 401 状态码
                //case StatusCodes.Status401Unauthorized:
                //    await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, errors: "401 Unauthorized")
                //        , AppHelper.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                //    break;
                //// 处理 403 状态码
                //case StatusCodes.Status403Forbidden:
                //    await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, errors: "403 Forbidden")
                //        , AppHelper.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                //    break;

                default: break;
            }
        }

        public IActionResult OnSucceeded(ActionExecutedContext context, object data)
        {
            return new JsonResult(RESTfulResult(StatusCodes.Status200OK, true, data)
           , UnifyContext.GetSerializerSettings(context));
        }

        public IActionResult OnValidateFailed(ActionExecutingContext context, ValidationMetadata metadata)
        {
            return new JsonResult(RESTfulResult(metadata.StatusCode ?? StatusCodes.Status400BadRequest, data: metadata.Data, errors: metadata.ValidationResult)
           , UnifyContext.GetSerializerSettings(context));
        }

        public IActionResult OnValidateFailed(ExceptionContext context, ValidationMetadata metadata)
        {
            return new JsonResult(RESTfulResult(metadata.StatusCode ?? StatusCodes.Status400BadRequest, data: metadata.Data, errors: metadata.ValidationResult)
           , UnifyContext.GetSerializerSettings(context));
        }

        /// <summary>
        /// 返回 RESTful 风格结果集
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="succeeded"></param>
        /// <param name="data"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public static AppRESTfulResult<object> RESTfulResult(int statusCode, bool succeeded = default, object data = default, object errors = default)
        {
            return new AppRESTfulResult<object>
            {
                StatusCode = statusCode,
                Succeeded = succeeded,
                Data = data,
                Errors = errors,
                Extras = UnifyContext.Take(),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }
    }

}
