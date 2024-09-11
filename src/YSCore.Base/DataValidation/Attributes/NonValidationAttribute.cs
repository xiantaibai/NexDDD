using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.DataValidation.Attributes
{
    /// <summary>
    /// 跳过验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class NonValidationAttribute : Attribute
    {
    }
}
