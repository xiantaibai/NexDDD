using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Diagnostics
{
    public static class YsNetCoreDiagnosticListenerNames
    {
        public static readonly DiagnosticListener DiagnosticListener =
       new(DiagnosticListenerName);

        public const string DiagnosticListenerName = "YsNetCore";

        public const string DomainEventHandlerBegin = "YsNetCore.DomainEventHandlerBegin";
        public const string DomainEventHandlerEnd = "YsNetCore.DomainEventHandlerEnd";
        public const string DomainEventHandlerError = "YsNetCore.DomainEventHandlerError";

        public const string IntegrationEventHandlerBegin = "YsNetCore.IntegrationEventHandlerBegin";
        public const string IntegrationEventHandlerEnd = "YsNetCore.IntegrationEventHandlerEnd";
        public const string IntegrationEventHandlerError = "YsNetCore.IntegrationEventHandlerError";

        public const string TransactionBegin = "YsNetCore.TransactionBegin";
        public const string TransactionCommit = "YsNetCore.TransactionCommit";
        public const string TransactionRollback = "YsNetCore.TransactionRollback";

        public const string CommandHandlerBegin = "YsNetCore.CommandHandlerBegin";
        public const string CommandHandlerEnd = "YsNetCore.CommandHandlerEnd";
        public const string CommandHandlerError = "YsNetCore.CommandHandlerError";
    }
}
