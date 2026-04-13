using Microsoft.AspNetCore.Mvc;
using MyDDD.Template.Api.Abstractions;
using MyDDD.Template.Api.Extensions;
using MyDDD.Template.Application.Users.LoginUser;
using MyDDD.Template.Domain.Primitives;
using Wolverine;

namespace MyDDD.Template.Api.Endpoints.Users;

public class UserEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("users")
            .WithTags("Users")
            .AllowAnonymous();

        group.MapPost("login", Login);
    }

    private static async Task<IResult> Login(
        [FromBody] LoginUserCommand command,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Result<LoginResponse>>(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails();
    }
}
