using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Diagnostics
{
    /// <summary>
    /// 负责NexDDD 核心模块中的 NexDddDiagnosticListenerNames 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public static class NexDddDiagnosticListenerNames
    {
        public static readonly DiagnosticListener DiagnosticListener =
       new(DiagnosticListenerName);

        public const string DiagnosticListenerName = "NexDDD";

        public const string DomainEventHandlerBegin = "NexDDD.DomainEventHandlerBegin";
        public const string DomainEventHandlerEnd = "NexDDD.DomainEventHandlerEnd";
        public const string DomainEventHandlerError = "NexDDD.DomainEventHandlerError";

        public const string IntegrationEventHandlerBegin = "NexDDD.IntegrationEventHandlerBegin";
        public const string IntegrationEventHandlerEnd = "NexDDD.IntegrationEventHandlerEnd";
        public const string IntegrationEventHandlerError = "NexDDD.IntegrationEventHandlerError";

        public const string TransactionBegin = "NexDDD.TransactionBegin";
        public const string TransactionCommit = "NexDDD.TransactionCommit";
        public const string TransactionRollback = "NexDDD.TransactionRollback";

        public const string CommandHandlerBegin = "NexDDD.CommandHandlerBegin";
        public const string CommandHandlerEnd = "NexDDD.CommandHandlerEnd";
        public const string CommandHandlerError = "NexDDD.CommandHandlerError";
    }
}
