using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using YSCore.Base.App;
using YSCore.Base.Exceptions.Extensions;
using YSCore.Base.Exceptions.Options;
using YSCore.Base.Extensions;
using YSCore.Base.Localization;

namespace YSCore.Base.Exceptions
{
    /// <summary>
    /// 异常帮助类。
    /// </summary>
    public static class ExceptionHelper
    {
        public static AppFriendlyException Bah(string errorMessage, Exception exceptionType, params object[] args)
        {
            var friendlyException = AppFriendly(errorMessage, exceptionType, args).StatusCode(StatusCodes.Status400BadRequest);
            friendlyException.ValidationException = true;
            return friendlyException;
        }

        public static AppFriendlyException Bah(string errorMessage, params object[] args)
        {
            var friendlyException = AppFriendly(errorMessage, typeof(ValidationException), args).StatusCode(StatusCodes.Status400BadRequest);
            friendlyException.ValidationException = true;
            return friendlyException;
        }

        public static AppFriendlyException Bah(object errorCode, params object[] args)
        {
            var friendlyException = AppFriendly(errorCode, typeof(ValidationException), args).StatusCode(StatusCodes.Status400BadRequest);
            friendlyException.ValidationException = true;
            return friendlyException;
        }

        public static AppFriendlyException AppFriendly(string errorMessage, params object[] args)
        {
            var friendlyException = new AppFriendlyException(ResolveErrorMessage(errorMessage, default, null, args), default);

            if (Settings.ThrowBah == true)
            {
                friendlyException.StatusCode(StatusCodes.Status400BadRequest);
                friendlyException.ValidationException = true;
            }

            return friendlyException;
        }

        public static AppFriendlyException AppFriendly(object errorCode, params object[] args)
        {
            var exceptionMessage = ResolveErrorMessage(default, errorCode, null, args);
            var friendlyException = new AppFriendlyException(exceptionMessage, errorCode);

            if (Settings.ThrowBah == true)
            {
                friendlyException.StatusCode(StatusCodes.Status400BadRequest);
                friendlyException.ValidationException = true;
            }

            return friendlyException;
        }

        public static AppFriendlyException AppFriendly(string errorMessage, Type exceptionType, params object[] args)
        {
            var exceptionMessage = ResolveErrorMessage(errorMessage, default, null, args);
            return new AppFriendlyException(exceptionMessage, default,
                Activator.CreateInstance(exceptionType, new object[] { exceptionMessage }) as Exception);
        }

        public static AppFriendlyException AppFriendly(object errorCode, Type exceptionType, params object[] args)
        {
            var exceptionMessage = ResolveErrorMessage(default, errorCode, null, args);
            return new AppFriendlyException(exceptionMessage, errorCode,
                Activator.CreateInstance(exceptionType, new object[] { exceptionMessage }) as Exception);
        }

        public static AppFriendlyException AppFriendly<TException>(string errorMessage, params object[] args)
            where TException : class
        {
            return AppFriendly(errorMessage, typeof(TException), args);
        }

        public static AppFriendlyException AppFriendly(string errorMessage, Exception exceptionType, params object[] args)
        {
            var exceptionMessage = ResolveErrorMessage(errorMessage, default, null, args);
            return new AppFriendlyException(exceptionMessage, default, exceptionType);
        }

        public static AppFriendlyException AppFriendly(object errorCode, Exception exceptionType, params object[] args)
        {
            var exceptionMessage = ResolveErrorMessage(default, errorCode, null, args);
            return new AppFriendlyException(exceptionMessage, errorCode, exceptionType);
        }

        /// <summary>
        /// 构建内部使用的详细异常对象。
        /// </summary>
        public static AppErrorInfo BuildErrorInfo(Exception exception, string traceId = null)
        {
            return BuildErrorInfo(exception, traceId, 0);
        }

        /// <summary>
        /// 构建返回给客户端的精简异常对象。
        /// </summary>
        public static AppErrorResponse BuildPublicError(AppErrorInfo error)
        {
            if (error == null) return null;

            return new AppErrorResponse
            {
                Code = string.IsNullOrWhiteSpace(error.Code) ? Settings.DefaultErrorCode : error.Code,
                Message = error.Message
            };
        }

        public static string ResolveErrorMessage(object errorMessage, object errorCode = null, bool? hideErrorCode = null, params object[] args)
        {
            var message = ResolveMessageValue(errorMessage, errorCode);
            var code = NormalizeErrorCode(errorCode);
            var withCode = hideErrorCode == true || Settings.HideErrorCode == true || string.IsNullOrWhiteSpace(code)
                ? message
                : $"[{code}] {message}";

            return args is { Length: > 0 }
                ? withCode.Format(args)
                : withCode;
        }

        public static string NormalizeErrorCode(object errorCode)
        {
            return errorCode?.ToString();
        }

        private static AppErrorInfo BuildErrorInfo(Exception exception, string traceId, int depth)
        {
            if (exception == null || depth > 5) return null;

            var friendlyException = exception as AppFriendlyException;
            var errorCode = NormalizeErrorCode(friendlyException?.ErrorCode);
            var originErrorCode = NormalizeErrorCode(friendlyException?.OriginErrorCode);
            var statusCode = friendlyException?.StatusCode ?? StatusCodes.Status500InternalServerError;
            var validationException = friendlyException?.ValidationException ?? false;
            var message = friendlyException != null
                ? ResolveErrorMessage(friendlyException.ErrorMessage, friendlyException.ErrorCode)
                : ResolveUnhandledExceptionMessage(exception);

            if (string.IsNullOrWhiteSpace(errorCode))
            {
                errorCode = Settings.DefaultErrorCode;
            }

            if (string.IsNullOrWhiteSpace(originErrorCode))
            {
                originErrorCode = errorCode;
            }

            var canExposeInner = friendlyException?.ExposeInnerException ?? (Settings.PropagateInnerException ?? true);

            return new AppErrorInfo
            {
                Code = errorCode,
                OriginCode = originErrorCode,
                Message = message,
                StatusCode = statusCode,
                ValidationException = validationException,
                TraceId = traceId,
                ExceptionType = exception.GetType().FullName,
                Data = friendlyException?.Data,
                InnerError = canExposeInner && exception.InnerException != null
                    ? BuildErrorInfo(exception.InnerException, traceId, depth + 1)
                    : null
            };
        }

        private static string ResolveUnhandledExceptionMessage(Exception exception)
        {
            if (exception == null)
            {
                return ResolveMessageValue(Settings.DefaultErrorMessage, Settings.DefaultErrorCode);
            }

            var rawMessage = exception.InnerException?.Message ?? exception.Message;
            return ResolveMessageValue(rawMessage, Settings.DefaultErrorCode);
        }

        private static string ResolveMessageValue(object errorMessage, object errorCode)
        {
            var message = errorMessage?.ToString();
            if (!string.IsNullOrWhiteSpace(message))
            {
                return Localize(message);
            }

            var code = NormalizeErrorCode(errorCode);
            if (!string.IsNullOrWhiteSpace(code))
            {
                if (Settings.ErrorCodeMessages != null && Settings.ErrorCodeMessages.TryGetValue(code, out var configuredMessage) && !string.IsNullOrWhiteSpace(configuredMessage))
                {
                    return Localize(configuredMessage);
                }

                var localizedCodeMessage = Localize(code, allowIdentity: false);
                if (!string.IsNullOrWhiteSpace(localizedCodeMessage))
                {
                    return localizedCodeMessage;
                }
            }

            var defaultMessage = Settings.DefaultErrorMessage;
            if (!string.IsNullOrWhiteSpace(defaultMessage))
            {
                return Localize(defaultMessage);
            }

            return !string.IsNullOrWhiteSpace(code) ? code : "服务异常";
        }

        private static string Localize(string value, bool allowIdentity = true)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var localizer = L.Text;
            if (localizer == null)
            {
                return value;
            }

            var localized = localizer[value];
            if (localized.ResourceNotFound && !allowIdentity)
            {
                return null;
            }

            return localized.Value;
        }

        private static FriendlyExceptionSettingsOptions Settings =>
            AppHelper.RootServices.GetService<IOptions<FriendlyExceptionSettingsOptions>>()?.Value
            ?? new FriendlyExceptionSettingsOptions();
    }
}
