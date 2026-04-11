using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MyDDD.Template.Application.Abstractions;

namespace MyDDD.Template.Infrastructure.Auth;

public sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId => Guid.Parse(httpContextAccessor.HttpContext?.User.FindFirstValue("userId") ??
                                    throw new UnauthorizedAccessException());

    public string IdentityId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        throw new UnauthorizedAccessException();

    public string? Email => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

    public string? FirstName => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.GivenName);

    public string? LastName => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Surname);
}
