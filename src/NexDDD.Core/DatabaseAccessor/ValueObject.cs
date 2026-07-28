using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.DatabaseAccessor
{
    /// <summary>
    /// 值对象
    /// 负责数据访问与聚合持久化模块中的 ValueObject 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    public abstract class ValueObject
    {
        protected static bool EqualOperator(ValueObject left, ValueObject right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            {
                return false;
            }
#pragma warning disable CS8604 // 引用类型参数可能为 null。
            return ReferenceEquals(left, null) || left.Equals(right);
#pragma warning restore CS8604 // 引用类型参数可能为 null。
        }

        protected static bool NotEqualOperator(ValueObject left, ValueObject right)
        {
            return !(EqualOperator(left, right));
        }

        /// <summary>
        /// 获取相等组件
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueObject)obj;

            return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        /// <summary>
        /// 根据原子值生成哈希值
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }

        public ValueObject GetCopy()
        {
#pragma warning disable CS8603 // 可能返回 null 引用。
            return this.MemberwiseClone() as ValueObject;
#pragma warning restore CS8603 // 可能返回 null 引用。
        }

    }
}
