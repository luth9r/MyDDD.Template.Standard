using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Domain.Projects;

namespace MyDDD.Template.Application.Projects.GetProjectById;

public sealed record GetProjectByIdQuery(Guid Id);

public static class GetProjectByIdQueryHandler
{
    public static async Task<Result<ProjectResponse>> Handle(
        GetProjectByIdQuery request,
        IProjectQueries projectQueries,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var projectResponse = await projectQueries.GetByIdAsync(request.Id, await userContext.GetUserIdAsync(cancellationToken), cancellationToken);

        if (projectResponse is null)
        {
            return Result.Failure<ProjectResponse>(MyError.NotFound(
                "Project.NotFound",
                $"Project with Id '{request.Id}' was not found."));
        }

        return Result.Success(projectResponse);
    }
}
