using Microsoft.EntityFrameworkCore;
using MyDDD.Template.Domain.Users;

namespace MyDDD.Template.Infrastructure.Persistence.Configurations.Domain.User;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<Template.Domain.Users.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Template.Domain.Users.User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<Template.Domain.Users.User?> GetByIdentityIdAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.IdentityId == identityId, cancellationToken);
    }

    public void Add(Template.Domain.Users.User user)
    {
        context.Users.Add(user);
    }
}
