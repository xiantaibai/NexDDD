using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.Extension;
using YSCore.Base.Idempotency;
using YSCore.Base.Events;

namespace YSCore.Base.Events
{
    /// <summary>
    /// 用于处理重复请求并确保幂等更新,客户端发送的 RequestID 用于检测重复请求。
    /// </summary>
    /// <typeparam name="T">IdentifiedCommand 的子类</typeparam>
    /// <typeparam name="R">命令处理程序的返回值</typeparam>
    public abstract class IdentifiedCommandHandler<T, R> : ICommandHandler<IdentifiedCommand<T, R>, R>
    where T : ICommand<R>
    {
        private readonly IMediator _mediator;
        private readonly IRequestManager _requestManager;
        private readonly ILogger<IdentifiedCommandHandler<T, R>> _logger;

        public IdentifiedCommandHandler(
            IMediator mediator,
            IRequestManager requestManager,
            ILogger<IdentifiedCommandHandler<T, R>> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _mediator = mediator;
            _requestManager = requestManager;
            _logger = logger;
        }

        /// <summary>
        /// 创建在找到上一个请求时返回的结果值
        /// </summary>
        /// <returns></returns>
        protected abstract R CreateResultForDuplicateRequest();

        public async Task<R> Handle(IdentifiedCommand<T, R> message, CancellationToken cancellationToken)
        {
            var alreadyExists = await _requestManager.ExistAsync(message.Id);
            if (alreadyExists)
            {
                return CreateResultForDuplicateRequest();
            }
            else
            {
                await _requestManager.CreateRequestForCommandAsync<T>(message.Id);
                try
                {
                    var command = message.Command;
                    var commandName = command.GetGenericTypeName();
                    var idProperty = string.Empty;
                    var commandId = string.Empty;

                    _logger.LogInformation(
                        "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                        commandName,
                        idProperty,
                        commandId,
                        command);

                    var result = await _mediator.Send(command, cancellationToken);

                    _logger.LogInformation(
                        "Command result: {@Result} - {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                        result,
                        commandName,
                        idProperty,
                        commandId,
                        command);

                    return result;
                }
                catch
                {
                    return default;
                }
            }
        }
    }
}
