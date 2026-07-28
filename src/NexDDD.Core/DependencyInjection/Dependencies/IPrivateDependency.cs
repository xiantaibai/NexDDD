using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.DependencyInjection.Dependencies
{
    /// <summary>
    /// 依赖空接口（禁止外部继承）
    /// 定义依赖注入模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface IPrivateDependency
    {
    }
}
