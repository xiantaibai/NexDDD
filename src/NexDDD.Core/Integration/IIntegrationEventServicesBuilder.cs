using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Integration
{
    /// <summary>
    /// 定义集成事件模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface IIntegrationEventServicesBuilder
    {
        IServiceCollection Services { get; }
    }
}
