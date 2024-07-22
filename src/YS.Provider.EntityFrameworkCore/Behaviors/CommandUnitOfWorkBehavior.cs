using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.Diagnostics;
using YSCore.Base.Events;

namespace YS.Provider.EntityFrameworkCore.Behaviors
{
    internal class CommandUnitOfWorkBehavior<TCommand, TResponse> : IPipelineBehavior<TCommand, TResponse>
        where TCommand : IBaseCommand
    {
#pragma warning disable S2743
        private static readonly DiagnosticListener _diagnosticListener =
#pragma warning restore S2743
            new DiagnosticListener(YsNetCoreDiagnosticListenerNames.DiagnosticListenerName);

        private readonly ITransactionUnitOfWork _unitOfWork;

        public CommandUnitOfWorkBehavior(ITransactionUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<TResponse> Handle(TCommand request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var cmdType = typeof(TCommand);
            var commandName = cmdType.FullName ?? cmdType.Name;
            Guid id = Guid.NewGuid();
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
