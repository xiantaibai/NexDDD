using testApi.Entites;
using testApi.MultiTenantDbContext;
using NexDDD.Core.CommandLocks;
using NexDDD.Core.Events;
using NexDDD.Core.Idempotency;

namespace testApi.Commands
{
    /// <summary>
    /// 承载示例与回归验证流程中的不可变消息数据，供处理器、管线或基础设施组件传递。
    /// </summary>
    public record CreateBlogCommand(string name, string code) : ICommand<bool>;

    /// <summary>
    /// 负责示例与回归验证流程的处理与编排，协调上游请求和下游服务完成当前用例。
    /// </summary>
    public class CreateBlogCommandHandlers(TestDbContext testDbContext) : ICommandHandler<CreateBlogCommand, bool>
    {
        public async Task<bool> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
        {
            var blog = new Blog(request.name, request.code);

            var res = await testDbContext.AddAsync(blog);

            return res != null;
        }
    }

    /// <summary>
    /// 用于验证示例与回归验证相关行为，确保框架重命名后契约和运行结果保持正确。
    /// </summary>
    public class CreateBlogCommandLock : ICommandLock<CreateBlogCommand>
    {
        public Task<CommandLockSettings> GetLockKeysAsync(CreateBlogCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CommandLockSettings(command.code, 10));
        }
    }

    /// <summary>
    /// 用于验证示例与回归验证相关行为，确保框架重命名后契约和运行结果保持正确。
    /// </summary>
    public class CreateBlogCommandIdempotent : IIdempotentCommand<CreateBlogCommand>
    {
        public Task<IdempotencySettings> GetIdempotencyAsync(CreateBlogCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new IdempotencySettings("blog:create", command.code, TimeSpan.FromSeconds(60)));
        }
    }
}
