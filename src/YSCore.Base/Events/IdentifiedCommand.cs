using MediatR;
using YSCore.Base.Events;

namespace YSCore.Base.Events
{
    /// <summary>
    /// 处理重复请求的指令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    public class IdentifiedCommand<T, R> : ICommand<R>
    where T : ICommand<R>
    {
        public T Command { get; }
        public Guid Id { get; }
        public IdentifiedCommand(T command, Guid id)
        {
            Command = command;
            Id = id;
        }
    }
}
