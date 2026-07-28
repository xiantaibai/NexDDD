using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexDDD.Core.DataValidation.Options;
using NexDDD.Core.Exceptions.Options;
using NexDDD.Core.UnifyResult.Options;

namespace NexDDD.Core.App.Options
{
    /// <summary>
    /// 汇总 NexDDD 核心能力的注册选项，并把验证与友好异常配置传递给对应模块。
    /// </summary>
    public sealed class AddNexDDDOptions
    {
        /// <summary>
        /// 配置 DataValidation
        /// </summary>
        /// <param name="configure"></param>
        public void ConfigureDataValidation(Action<DataValidationOptions> configure)
        {
            DataValidationConfigure = configure;
        }

        /// <summary>
        /// 配置 FriendlyException
        /// </summary>
        /// <param name="configure"></param>
        public void ConfigureFriendlyException(Action<FriendlyExceptionOptions> configure)
        {
            FriendlyExceptionConfigure = configure;
        }

        /// <summary>
        /// DataValidation 配置
        /// </summary>
        internal static Action<DataValidationOptions> DataValidationConfigure { get; private set; }

        /// <summary>
        /// FriendlyException 配置
        /// </summary>
        internal static Action<FriendlyExceptionOptions> FriendlyExceptionConfigure { get; private set; }

    }
}
