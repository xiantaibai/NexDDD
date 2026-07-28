using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Events
{
    /// <summary>
    /// 定义命令调度模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface IBaseCommand { }

    /// <summary>
    /// 定义命令调度模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface ICommand : IRequest, IBaseCommand { }


    /// <summary>
    /// 定义命令调度模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface ICommand<out TResponse> : IRequest<TResponse>, IBaseCommand { }
}
