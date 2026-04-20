using MyDDD.Template.Application.Projects;

namespace MyDDD.Template.Application.Abstractions;

/// <summary>
/// Readonly queries for project dto
/// </summary>
public interface IProjectQueries
{
    Task<ProjectResponse?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
}
