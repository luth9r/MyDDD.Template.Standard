using FluentValidation;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Application.Abstractions.Messaging;

namespace MyDDD.Template.Application.Users.LoginUser;

public record LoginUserCommand(string Email, string Password) : ICommand<AccessTokenResponse>;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}

internal sealed class LoginUserCommandHandler(IIdentityService identityService)
    : ICommandHandler<LoginUserCommand, AccessTokenResponse>
{
    public async Task<Result<AccessTokenResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<AccessTokenResponse>(result.Error);
        }

        return result.Value;
    }
}
