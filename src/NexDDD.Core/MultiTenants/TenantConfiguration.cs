using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.MultiTenants
{

    /// <summary>
    /// 承载多租户模块的运行配置，供依赖注入注册过程与运行时组件读取。
    /// </summary>
    public class TenantSettings<T> where T : Tenant
    {
        /// <summary>
        /// 获取或设置未命中特定租户时使用的默认数据库连接配置。
        /// </summary>
        public ConnectionStrings Defaults { get; set; }

        /// <summary>
        /// 获取或设置是否禁止自动注册多租户管线，默认允许自动注册。
        /// </summary>
        public bool DisableAutomaticPipelineRegistration { get; set; } = false;

        /// <summary>
        /// 获取或设置可用租户集合，每个租户必须具有唯一标识和连接配置。
        /// </summary>
        public List<T> Tenants { get; set; }
    }

    /// <summary>
    /// 负责多租户模块中的 Tenant 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public class Tenant
    {
        /// <summary>
        /// 获取或设置租户唯一标识。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置租户显示名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置租户专属数据库连接字符串；该值可能包含凭据，不应写入日志。
        /// </summary>
        public string ConnectionString { get; set; }
    }

    /// <summary>
    /// 负责多租户模块中的 ConnectionStrings 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public class ConnectionStrings
    {
        /// <summary>
        /// 获取或设置数据库提供程序名称，用于选择对应的 DbContext 配置方式。
        /// </summary>
        public string DBProvider { get; set; }

        /// <summary>
        /// 获取或设置默认数据库连接字符串；该值可能包含凭据，不应写入日志。
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
