namespace MyDDD.Template.Domain.Projects;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Project?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    void Add(Project project);

    void Update(Project project);

    void Remove(Project project);
}
