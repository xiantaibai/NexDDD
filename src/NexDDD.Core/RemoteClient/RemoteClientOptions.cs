using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NexDDD.Core.RemoteClient
{
    /// <summary>
    /// 承载远程客户端模块的运行配置，供依赖注入注册过程与运行时组件读取。
    /// </summary>
    public class RemoteClientOptions
    {
        /// <summary>
        /// 获取或设置远程服务基础地址，必须是 HttpClient 支持的有效绝对或相对地址。
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// 获取或设置请求与响应使用的 JSON 序列化选项，默认采用 System.Text.Json 默认设置。
        /// </summary>
        public JsonSerializerOptions? JsonSerializerSettings { get; set; } = new JsonSerializerOptions();

        /// <summary>
        /// 获取或设置远程请求超时时间，默认 30 秒；过大会延长故障恢复时间。
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
