using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.DataValidation.Options;
using YSCore.Base.Exceptions.Options;
using YSCore.Base.UnifyResult.Options;

namespace YSCore.Base.App.Options
{
    /// <summary>
    /// 配置项
    /// </summary>
    public sealed class AddYsCoreOptions
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
