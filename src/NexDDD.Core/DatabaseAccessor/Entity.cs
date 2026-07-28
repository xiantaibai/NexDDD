using MediatR;
using NexDDD.Core.DomainEvent;

namespace NexDDD.Core.DatabaseAccessor
{
    /// <summary>
    /// 实体基类
    /// 负责数据访问与聚合持久化模块中的 Entity 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    public abstract class Entity
    {
        public abstract object[] GetKeys();


        public override string ToString() => $"[Entity: {GetType().Name}] Keys = {string.Join(",", GetKeys())}";

        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyList<IDomainEvent> GetDomainEvents()
        {
            return _domainEvents.AsReadOnly();
        }

        public void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(IDomainEvent eventItem) => _domainEvents.Remove(eventItem);

        public void ClearDomainEvents() => _domainEvents.Clear();
    }

    /// <summary>
    /// 负责数据访问与聚合持久化模块中的 Entity 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public abstract class Entity<TKey> : Entity where TKey : notnull
    {
        int? _requestedHashCode;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public virtual TKey Id { get; protected set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public override object[] GetKeys() => new object[] { Id };

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is not Entity<TKey>)
            {
                return false;
            }

            if (GetType() != obj.GetType())
            {
                return false;
            }

            var item = (Entity<TKey>)obj;

            if (item.IsTransient() || IsTransient())
            {
                return false;
            }
            else
            {
                if (Id == null)
                {
                    return false;
                }
                return Id.Equals(item.Id);
            }
        }

        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                if (!_requestedHashCode.HasValue)
                    _requestedHashCode = this.Id.GetHashCode() ^ 31;

                return _requestedHashCode.Value;
            }
            else
                return base.GetHashCode();
        }


        //表示对象是否为全新创建的，未持久化的
        public bool IsTransient() => EqualityComparer<TKey>.Default.Equals(Id, default);

        public override string ToString() => $"[Entity: {GetType().Name}] Id = {Id}";
    }
}
