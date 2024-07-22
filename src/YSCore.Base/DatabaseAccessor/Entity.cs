using MediatR;
using YSCore.Base.DomainEvent;

namespace YSCore.Base.DatabaseAccessor
{
    /// <summary>
    /// 实体基类
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
