using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MyDDD.Template.Api.Abstractions;
using MyDDD.Template.Api.Extensions;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Application.Users.LoginUser;
using MyDDD.Template.Application.Users.RegisterUser;

namespace MyDDD.Template.Api.Endpoints.Users;

public class UserEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("users")
            .WithTags("Users")
            .AllowAnonymous();

        group.MapPost("register", Register);

        group.MapPost("login", Login);
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterUserRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails();
    }

    private sealed record RegisterUserRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName);

    private static async Task<IResult> Login(
        [FromBody] LoginUserCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails();
    }
}
