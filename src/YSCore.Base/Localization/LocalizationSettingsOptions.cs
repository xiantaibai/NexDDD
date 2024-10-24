using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Localization
{
    public class LocalizationSettingsOptions
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string ResourcesPath { get; set; }

        /// <summary>
        /// 支持的语言列表
        /// </summary>
        public string[] SupportedCultures { get; set; }

        /// <summary>
        /// 默认的语言
        /// </summary>
        public string DefaultCulture { get; set; }

        /// <summary>
        /// 资源文件名前缀
        /// </summary>
        public string LanguageFilePrefix { get; set; }

        /// <summary>
        /// 资源所在程序集名称
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// 设置 DateTime 格式化标准语言
        /// </summary>
        /// <remarks>不设置则自动根据当前语言</remarks>
        public string DateTimeFormatCulture { get; set; }
    }
}
