using FluentValidation;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Domain;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Domain.Users;
using MyDDD.Template.Application.Abstractions.Messaging;

namespace MyDDD.Template.Application.Users.RegisterUser;

public record RegisterUserCommand(string Email, string Password, string FirstName, string LastName) : ICommand<Guid>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(p => p.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(p => p.Password)
            .Length(6, 20)
            .NotEmpty();

        RuleFor(p => p.FirstName)
            .NotEmpty();

        RuleFor(p => p.LastName)
            .NotEmpty();
    }
}

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IIdentityService identityService)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(Error.Conflict("User.DuplicateEmail", "User with this email already exists"));
        }
        var internalUserId = Guid.NewGuid();

        var identityResult = await identityService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            internalUserId,
            cancellationToken);

        if (identityResult.IsFailure)
        {
            return Result.Failure<Guid>(identityResult.Error);
        }

        var user = User.Create(
            internalUserId,
            identityResult.Value,
            request.Email,
            request.FirstName,
            request.LastName);

        userRepository.Add(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return internalUserId;
    }
}
