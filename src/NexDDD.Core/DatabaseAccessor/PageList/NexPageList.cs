using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.DatabaseAccessor.PageList
{
    /// <summary>
    /// 分页泛型集合
    /// 负责数据访问与聚合持久化模块中的 NexPageList 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class NexPageList<TEntity>
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 页容量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 当前页集合
        /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public IEnumerable<TEntity> Items { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPrevPage { get; set; }

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage { get; set; }
    }
}
