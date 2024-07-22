using MediatR;
using YSCore.Base.Events;

namespace YSCore.Base.Events
{
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
