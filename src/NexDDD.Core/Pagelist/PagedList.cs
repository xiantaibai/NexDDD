using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Pagelist
{
    /// <summary>
    /// 负责NexDDD 核心模块中的 PagedList 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public class PagedList<T>
    {
        public List<T> Items { get; private set; }
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageNumber = pageNumber;
            PageSize = pageSize;
            Items = items;
        }

        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;

       
    }
}
