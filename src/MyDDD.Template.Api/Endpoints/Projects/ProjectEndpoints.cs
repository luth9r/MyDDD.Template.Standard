using MyDDD.Template.Api.Abstractions;
using MyDDD.Template.Api.Extensions;
using MyDDD.Template.Application.Projects;
using MyDDD.Template.Application.Projects.CreateProject;
using MyDDD.Template.Application.Projects.GetAllUserProjects;
using MyDDD.Template.Application.Projects.GetProjectById;
using MyDDD.Template.Domain.Primitives;
using Wolverine;

namespace MyDDD.Template.Api.Endpoints.Projects;

public sealed class ProjectEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("projects")
            .WithTags("Projects")
            .RequireAuthorization();

        group.MapPost(string.Empty, CreateProject)
            .WithName("CreateProject");

        group.MapGet("{id:guid}", GetProject)
            .WithName("GetProject");

        group.MapGet(string.Empty, GetAllUserProjects)
            .WithName("GetAllUserProjects");
    }

    private static async Task<IResult> CreateProject(
        CreateProjectCommand command,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Result<Guid>>(command, cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute("GetProject", new { id = result.Value }, result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> GetProject(
        Guid id,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Result<ProjectResponse>>(new GetProjectByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> GetAllUserProjects(
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result =
            await bus.InvokeAsync<Result<IReadOnlyList<ProjectResponse>>>(
                new GetAllUserProjectsQuery(),
                cancellationToken);

        return Results.Ok(result.Value);
    }
}
