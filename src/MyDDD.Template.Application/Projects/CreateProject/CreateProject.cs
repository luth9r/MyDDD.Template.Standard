using FluentValidation;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Application.Abstractions.Messaging;
using MyDDD.Template.Domain;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Domain.Projects;

namespace MyDDD.Template.Application.Projects.CreateProject;

public sealed record CreateProjectCommand(string Name) : ICommand<Guid>;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(50);
    }
}

internal sealed class CreateProjectCommandHandler(
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : ICommandHandler<CreateProjectCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {

        if (await projectRepository.GetByNameAsync(request.Name, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(Error.Conflict(
                "Project.DuplicateName",
                $"Project with name '{request.Name}' already exists"));
        }

        var project = Project.Create(request.Name, userContext.UserId);

        projectRepository.Add(project);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(project.Id);
    }
}
