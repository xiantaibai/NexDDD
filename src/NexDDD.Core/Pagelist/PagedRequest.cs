using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NexDDD.Core.Pagelist
{
    /// <summary>
    /// 承载NexDDD 核心流程中的不可变消息数据，供处理器、管线或基础设施组件传递。
    /// </summary>
    public record SortField(string Field, bool Descending);

    /// <summary>
    /// 分页请求基类
    /// 负责NexDDD 核心模块中的 PagedRequest 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    public class PagedRequest
    {
        private int _pageIndex;

        private int _pageSize = 0;

        /// <summary>
        /// 当前页码
        /// </summary>
        [Description("当前页码")]
        public int PageIndex
        {
            get => _pageIndex;

            set => _pageIndex = value > 0 ? value : 1;
        }

        /// <summary>
        /// 每页记录数
        /// </summary>
        [Description("每页记录数")]
        public int PageSize
        {
            get => _pageSize;

            set => _pageSize = value > 10000 ? 15 : (value <= 0 ? 0 : value);
        }

        /// <summary>
        /// 排序字段（格式：-createTime,totalAmount）
        /// </summary>
        [Description("排序字段")]
        public string? Sort { get; set; }

        /// <summary>
        /// 解析排序参数为字段与方向
        /// </summary>
        [JsonIgnore]
        public List<SortField> SortFields => ParseSort(Sort);

        public static List<SortField> ParseSort(string? sort)
        {
            if (string.IsNullOrWhiteSpace(sort))
                return new();

            return sort.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .Where(s => !string.IsNullOrEmpty(s))
                       .Select(s => s.StartsWith('-')
                           ? new SortField(s[1..], true)
                           : new SortField(s, false))
                       .ToList();
        }
    }
}
