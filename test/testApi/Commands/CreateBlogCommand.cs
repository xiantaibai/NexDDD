using testApi.Entites;
using testApi.MultiTenantDbContext;
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
}
