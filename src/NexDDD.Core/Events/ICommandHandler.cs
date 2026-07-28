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
    public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand> where TCommand : ICommand
    {
    }

    /// <summary>
    /// 定义命令调度模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
    {
    }
}
