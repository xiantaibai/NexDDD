using testApi.Entites;
using testApi.MultiTenantDbContext;
using YSCore.Base.CommandLocks;
using YSCore.Base.Events;
using YSCore.Base.Idempotency;

namespace testApi.Commands
{
    public record CreateBlogCommand(string name, string code) : ICommand<bool>;

    public class CreateBlogCommandHandlers(TestDbContext testDbContext) : ICommandHandler<CreateBlogCommand, bool>
    {
        public async Task<bool> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
        {
            var blog = new Blog(request.name, request.code);

            var res = await testDbContext.AddAsync(blog);

            return res != null;
        }
    }

    public class CreateBlogCommandLock : ICommandLock<CreateBlogCommand>
    {
        public Task<CommandLockSettings> GetLockKeysAsync(CreateBlogCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CommandLockSettings(command.code, 10));
        }
    }

    public class CreateBlogCommandIdempotent : IIdempotentCommand<CreateBlogCommand>
    {
        public Task<IdempotencySettings> GetIdempotencyAsync(CreateBlogCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new IdempotencySettings("blog:create", command.code, TimeSpan.FromSeconds(60)));
        }
    }
}
