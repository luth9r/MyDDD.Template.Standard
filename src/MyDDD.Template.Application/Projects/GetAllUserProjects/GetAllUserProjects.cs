using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Application.Projects.GetAllUserProjects;

public sealed record GetAllUserProjectsQuery;

public static class GetAllUserProjectsQueryHandler
{
    public static async Task<Result<IReadOnlyList<ProjectResponse>>> Handle(
        GetAllUserProjectsQuery request,
        IProjectQueries projectQueries,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var projectResponse =
            await projectQueries.GetAllAsync(await userContext.GetUserIdAsync(cancellationToken), cancellationToken);

        return Result.Success(projectResponse);
    }
}
