using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.DependencyInjection.Dependencies
{
    /// <summary>
    /// 瞬时服务注册依赖
    /// </summary>
    public interface ITransient : IPrivateDependency
    {
    }
}
