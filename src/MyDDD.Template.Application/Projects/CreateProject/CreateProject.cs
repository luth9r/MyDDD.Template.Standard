using FluentValidation;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Domain;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Domain.Projects;

namespace MyDDD.Template.Application.Projects.CreateProject;

public sealed record CreateProjectCommand(string Name);

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(50);
    }
}

[Wolverine.Attributes.Transactional]
public static class CreateProjectCommandHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateProjectCommand request,
        IProjectRepository projectRepository,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        if (await projectRepository.GetByNameAsync(request.Name, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(MyError.Conflict(
                "Project.DuplicateName",
                $"Project with name '{request.Name}' already exists"));
        }

        var userId = await userContext.GetUserIdAsync(cancellationToken);

        var project = Project.Create(request.Name, userId);

        projectRepository.Add(project);

        return Result.Success(project.Id);
    }
}
