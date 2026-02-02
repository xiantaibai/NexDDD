using testApi.Entites;
using testApi.MultiTenantDbContext;
using YSCore.Base.CommandLocks;
using YSCore.Base.Events;

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
}
