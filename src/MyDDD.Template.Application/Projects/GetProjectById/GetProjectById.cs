using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Application.Abstractions.Messaging;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Domain.Projects;

namespace MyDDD.Template.Application.Projects.GetProjectById;

public sealed record GetProjectByIdQuery(Guid Id) : ICachedQuery<ProjectResponse>
{
    public string CacheKey => $"project-{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}

public sealed record ProjectResponse(Guid Id, string Name)
{
    public static ProjectResponse Map(Project project)
    {
        return new ProjectResponse(project.Id, project.Name);
    }
}

internal sealed class GetProjectByIdQueryHandler(
    IProjectRepository projectRepository,
    IUserContext userContext)
    : IQueryHandler<GetProjectByIdQuery, ProjectResponse>
{
    public async Task<Result<ProjectResponse>> Handle(
        GetProjectByIdQuery request,
        CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(request.Id, cancellationToken);

        if (project is null || project.UserId != userContext.UserId)
        {
            return Result.Failure<ProjectResponse>(Error.NotFound(
                "Project.NotFound",
                $"Project with Id '{request.Id}' was not found."));
        }

        return Result.Success(ProjectResponse.Map(project));
    }
}
