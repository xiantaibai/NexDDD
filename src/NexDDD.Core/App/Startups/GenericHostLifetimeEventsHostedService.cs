using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.App.Startups
{
    /// <summary>
    /// 监听泛型主机启动事件
    /// 负责命令调度模块中的 GenericHostLifetimeEventsHostedService 能力，并限定其与调用方及基础设施组件的协作边界。
    /// </summary>
    internal class GenericHostLifetimeEventsHostedService : IHostedService
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host"></param>
        public GenericHostLifetimeEventsHostedService(IHost host)
        {
            // 存储根服务
            AppHelper.SetRootServices(host.Services);
        }

        /// <summary>
        /// 监听主机启动
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 监听主机停止
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
