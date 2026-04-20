using Microsoft.EntityFrameworkCore;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Application.Projects;

namespace MyDDD.Template.Infrastructure.Persistence.Configurations.Projects;

public sealed class ProjectQueries(ApplicationDbContext context) : IProjectQueries
{
    public async Task<ProjectResponse?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Projects
            .AsNoTracking()
            .Where(p => p.Id == id && p.UserId == userId)
            .Select(p => new ProjectResponse(p.Id, p.Name))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Projects
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new ProjectResponse(p.Id, p.Name))
            .ToListAsync(cancellationToken);
    }
}
