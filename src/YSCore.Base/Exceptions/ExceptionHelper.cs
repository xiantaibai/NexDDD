using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using YSCore.Base.Exceptions.Extensions;
using YSCore.Base.Exceptions.Options;
using YSCore.Base.App;
using YSCore.Base.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using YSCore.Base.Localization;

namespace YSCore.Base.Exceptions
{
    /// <summary>
    /// 异常帮助类
    /// </summary>
    public static class ExceptionHelper
    {
        /// <summary>
        /// 友好异常设置
        /// </summary>
        private static readonly FriendlyExceptionSettingsOptions _friendlyExceptionSettings;

        static ExceptionHelper()
        {
            _friendlyExceptionSettings = AppHelper.RootServices.GetService<IOptions<FriendlyExceptionSettingsOptions>>().Value;
        }

        /// <summary>
        /// 抛出业务异常信息
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="exceptionType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static AppFriendlyException Bah(string errorMessage, Exception exceptionType, params object[] args)
        {
            var friendlyException = AppFriendly(errorMessage, exceptionType, args).StatusCode(StatusCodes.Status400BadRequest);
            friendlyException.ValidationException = true;
            return friendlyException;
        }

        /// <summary>
        /// 抛出业务异常信息
        /// </summary>
        /// <param name="errorMessage">异常消息</param>
        /// <param name="args">String.Format 参数</param>
        /// <returns>异常实例</returns>
        public static AppFriendlyException Bah(string errorMessage, params object[] args)
        {
            var friendlyException = AppFriendly(errorMessage, typeof(ValidationException), args).StatusCode(StatusCodes.Status400BadRequest);
            friendlyException.ValidationException = true;
            return friendlyException;
        }



        /// <summary>
        /// 抛出业务异常信息
        /// </summary>
        /// <param name="errorCode">错误码</param>
        /// <param name="args">String.Format 参数</param>
        /// <returns>异常实例</returns>
        //public static AppFriendlyException Bah(object errorCode, params object[] args)
        //{
        //    var friendlyException = AppFriendly(errorCode, typeof(ValidationException), args).StatusCode(StatusCodes.Status400BadRequest);
        //    friendlyException.ValidationException = true;
        //    return friendlyException;
        //}

        /// <summary>
        /// 抛出字符串异常
        /// </summary>
        /// <param name="errorMessage">异常消息</param>
        /// <param name="args">String.Format 参数</param>
        /// <returns>异常实例</returns>
        public static AppFriendlyException AppFriendly(string errorMessage, params object[] args)
        {
            var friendlyException = new AppFriendlyException(MontageErrorMessage(errorMessage, default, null, args), default);

            // 处理默认配置为业务异常问题
            if (_friendlyExceptionSettings.ThrowBah == true)
            {
                friendlyException.StatusCode(StatusCodes.Status400BadRequest);
                friendlyException.ValidationException = true;
            }
            return friendlyException;
        }

        public static AppFriendlyException AppFriendly(string errorMessage, Type exceptionType, params object[] args)
        {
            var exceptionMessage = MontageErrorMessage(errorMessage, default, null, args);
            return new AppFriendlyException(exceptionMessage, default,
                Activator.CreateInstance(exceptionType, new object[] { exceptionMessage }) as Exception);
        }

        /// <summary>
        /// 抛出字符串异常
        /// </summary>
        /// <typeparam name="TException">具体异常类型</typeparam>
        /// <param name="errorMessage">异常消息</param>
        /// <param name="args">String.Format 参数</param>
        /// <returns>异常实例</returns>
        public static AppFriendlyException AppFriendly<TException>(string errorMessage, params object[] args)
            where TException : class
        {
            return AppFriendly(errorMessage, typeof(TException), args);
        }

        public static AppFriendlyException AppFriendly(string errorMessage, Exception exceptionType, params object[] args)
        {
            var exceptionMessage = MontageErrorMessage(errorMessage, default, null, args);
            return new AppFriendlyException(exceptionMessage, default, exceptionType);
        }

        /// <summary>
        /// 获取错误码字符串
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        /// <param name="hideErrorCode">隐藏错误码</param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static string MontageErrorMessage(string errorMessage, string errorCode, bool? hideErrorCode, params object[] args)
        {
            // 支持读取配置渲染
            var realErrorMessage = errorMessage;

            // 多语言处理
            realErrorMessage = L.Text == null ? realErrorMessage : L.Text[realErrorMessage];

            // 判断是否隐藏错误码
            var msg = (hideErrorCode == true || _friendlyExceptionSettings.HideErrorCode == true || string.IsNullOrWhiteSpace(errorCode)
                ? string.Empty
                : $"[{errorCode}] ") + realErrorMessage;

            return msg.Format(args);
        }
    }
}
