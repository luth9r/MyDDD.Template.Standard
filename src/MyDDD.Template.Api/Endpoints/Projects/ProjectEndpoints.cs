using MediatR;
using MyDDD.Template.Api.Abstractions;
using MyDDD.Template.Api.Extensions;
using MyDDD.Template.Application.Abstractions.Messaging;
using MyDDD.Template.Application.Projects.CreateProject;
using MyDDD.Template.Application.Projects.GetProjectById;

namespace MyDDD.Template.Api.Endpoints.Projects;

public sealed class ProjectEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("projects")
            .WithTags("Projects")
            .RequireAuthorization();

        group.MapPost("", CreateProject);
        group.MapGet("{id:guid}", GetProject)
            .WithName("GetProject");
    }

    private static async Task<IResult> CreateProject(
        CreateProjectCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute("GetProject", new { id = result.Value }, result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> GetProject(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProjectByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails();
    }
}
