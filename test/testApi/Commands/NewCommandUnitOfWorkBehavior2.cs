using MediatR;
using NexDDD.Provider.EntityFrameworkCore;
using NexDDD.Core.Diagnostics;
using NexDDD.Core.Events;
using ShardingCore.Core.DbContextCreator;
using testApi.MultiTenantDbContext;

namespace testApi.Commands
{
    /// <summary>
    /// 负责示例与回归验证流程的处理与编排，协调上游请求和下游服务完成当前用例。
    /// </summary>
    public class NewCommandUnitOfWorkBehavior2<TCommand, TResponse> : IPipelineBehavior<TCommand, TResponse>
        where TCommand : IBaseCommand
    {
        private readonly TestDbContext _defaultTableDbContext;
        private readonly ILogger<NewCommandUnitOfWorkBehavior<TCommand, TResponse>> _logger;

        public NewCommandUnitOfWorkBehavior2(TestDbContext dbContext,ILogger<NewCommandUnitOfWorkBehavior<TCommand, TResponse>> logger)
        {
            _defaultTableDbContext = dbContext;
            _logger = logger;

        }

        public async Task<TResponse> Handle(TCommand request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var cmdType = typeof(TCommand);
            var commandName = cmdType.FullName ?? cmdType.Name;
            Guid id = Guid.NewGuid();
            if (_defaultTableDbContext.CurrentTransaction != null)
            {
                try
                {
                    _logger.LogInformation($"事务开始:{commandName},Id:{id},参数:{@request}");
                    var response = await next();
                    _logger.LogInformation($"事务结束:{commandName},Id:{id},参数:{@request}");
                    await _defaultTableDbContext.SaveEntitiesAsync(cancellationToken);
                    return response;
                }
                catch (Exception e)
                {
                    // WriteCommandError(new CommandError(id, commandName, request, e));
                    _logger.LogError(e,$"事务异常:{commandName},Id:{id},参数:{@request}");
                    throw;
                }
            }


            await using var transaction = _defaultTableDbContext.BeginTransaction();
            try
            {
                _logger.LogInformation($"事务开始:{commandName},Id:{id},参数:{@request}");
                var response = await next();
                _logger.LogInformation($"事务结束:{commandName},Id:{id},参数:{@request}");
                await _defaultTableDbContext.SaveEntitiesAsync(cancellationToken);
                await _defaultTableDbContext.CommitAsync(cancellationToken);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e,$"事务异常:{commandName},Id:{id},参数:{@request}");
                await _defaultTableDbContext.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
