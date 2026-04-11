using Microsoft.EntityFrameworkCore;
using MyDDD.Template.Domain.Projects;

namespace MyDDD.Template.Infrastructure.Persistence.Configurations.Domain.Projects;

internal sealed class ProjectRepository(ApplicationDbContext context) : IProjectRepository
{
    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Projects
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Project?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Projects
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public void Add(Project project)
    {
        context.Projects.Add(project);
    }

    public void Update(Project project)
    {
        context.Projects.Update(project);
    }

    public void Remove(Project project)
    {
        context.Projects.Remove(project);
    }
}
