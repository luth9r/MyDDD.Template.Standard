using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Application.Abstractions.Messaging;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Domain.Projects;

namespace MyDDD.Template.Application.Projects.GetProjectById;

public sealed record GetProjectByIdQuery(Guid Id);

public sealed record ProjectResponse(Guid Id, string Name)
{
    public static ProjectResponse Map(Project project)
    {
        return new ProjectResponse(project.Id, project.Name);
    }
}

public static class GetProjectByIdQueryHandler
{
    public static async Task<Result<ProjectResponse>> Handle(
        GetProjectByIdQuery request,
        IProjectRepository projectRepository,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(request.Id, cancellationToken);

        var userId = await userContext.GetUserIdAsync(cancellationToken);

        if (project is null || project.UserId != userId)
        {
            return Result.Failure<ProjectResponse>(MyError.NotFound(
                "Project.NotFound",
                $"Project with Id '{request.Id}' was not found."));
        }

        return Result.Success(ProjectResponse.Map(project));
    }
}
