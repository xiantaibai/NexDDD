using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using YSCore.Base.Context;
using YSCore.Base.Diagnostics;
using YSCore.Base.Events;
using YSCore.Base.Idempotency;

namespace YSCore.Provider.EntityFrameworkCore.Behaviors
{
    internal class CommandUnitOfWorkBehavior<TCommand, TResponse> : IPipelineBehavior<TCommand, TResponse>
        where TCommand : IBaseCommand
    {
#pragma warning disable S2743
        private static readonly DiagnosticListener _diagnosticListener =
#pragma warning restore S2743
            new DiagnosticListener(YsNetCoreDiagnosticListenerNames.DiagnosticListenerName);

        private readonly ITransactionUnitOfWork _unitOfWork;
        private readonly IContextAccessor<IdempotencyExecutionContext>? _idempotencyContextAccessor;
        private readonly IIdempotencyStore? _idempotencyStore;

        public CommandUnitOfWorkBehavior(ITransactionUnitOfWork unitOfWork, IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _idempotencyContextAccessor = serviceProvider.GetRequiredService<IContextAccessor<IdempotencyExecutionContext>>();
            _idempotencyStore = serviceProvider.GetService<IIdempotencyStore>();
        }

        public async Task<TResponse> Handle(TCommand request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var cmdType = typeof(TCommand);
            var commandName = cmdType.FullName ?? cmdType.Name;
            var id = Guid.NewGuid();

            if (_unitOfWork.CurrentTransaction != null)
            {
                try
                {
                    WriteCommandBegin(new CommandBegin(id, commandName, request));
                    var response = await next();
                    WriteCommandEnd(new CommandEnd(id, commandName, request));
                    await _unitOfWork.SaveEntitiesAsync(cancellationToken);
                    return response;
                }
                catch (Exception e)
                {
                    WriteCommandError(new CommandError(id, commandName, request, e));
                    throw;
                }
            }

            await using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                WriteCommandBegin(new CommandBegin(id, commandName, request));
                var response = await next();
                WriteCommandEnd(new CommandEnd(id, commandName, request));
                await _unitOfWork.SaveEntitiesAsync(cancellationToken);

                // 幂等组件存在时
                // 自动处理 Completed
                await TryMarkIdempotencyCompletedAsync(
                    cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return response;
            }
            catch (Exception e)
            {
                WriteCommandError(new CommandError(id, commandName, request, e));
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// 尝试提交幂等完成状态
        /// </summary>
        private async Task TryMarkIdempotencyCompletedAsync(
            CancellationToken cancellationToken)
        {
            // 未启用幂等组件
            // 直接跳过

            if (_idempotencyContextAccessor == null
                || _idempotencyStore == null)
            {
                return;
            }

            // 当前请求没有幂等上下文
            // 说明不是幂等请求

            if (!_idempotencyContextAccessor.HasContext)
            {
                return;
            }

            var context = _idempotencyContextAccessor.Current;

            if (context == null)
            {
                return;
            }

            await _idempotencyStore.MarkCompletedAsync(context.TypeCode,context.KeyHash,context.RequestId,cancellationToken);
        }

        void WriteCommandBegin(CommandBegin data)
        {
            if (_diagnosticListener.IsEnabled(YsNetCoreDiagnosticListenerNames.CommandHandlerBegin))
            {
                _diagnosticListener.Write(YsNetCoreDiagnosticListenerNames.CommandHandlerBegin, data);
            }
        }

        void WriteCommandEnd(CommandEnd data)
        {
            if (_diagnosticListener.IsEnabled(YsNetCoreDiagnosticListenerNames.CommandHandlerEnd))
            {
                _diagnosticListener.Write(YsNetCoreDiagnosticListenerNames.CommandHandlerEnd, data);
            }
        }

        void WriteCommandError(CommandError data)
        {
            if (_diagnosticListener.IsEnabled(YsNetCoreDiagnosticListenerNames.CommandHandlerError))
            {
                _diagnosticListener.Write(YsNetCoreDiagnosticListenerNames.CommandHandlerError, data);
            }
        }
    }
}
