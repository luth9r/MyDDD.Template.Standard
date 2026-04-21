using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Domain.Users;

namespace MyDDD.Template.Infrastructure.Auth;

public sealed class UserContext(
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider serviceProvider) : IUserContext
{
    private Guid? _cachedUserId;

    public string? Email => httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Email);

    public string? FirstName => httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.GivenName);

    public string? LastName => httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.FamilyName);

    public string IdentityId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
        throw new UnauthorizedAccessException();

    public async Task<Guid> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedUserId.HasValue)
        {
            return _cachedUserId.Value;
        }

        var httpContext = httpContextAccessor.HttpContext;

        var claimValue = httpContext?.User.FindFirstValue("userId");
        if (Guid.TryParse(claimValue, out var userId))
        {
            _cachedUserId = userId;
            return userId;
        }

        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();

        var user = await userRepository.GetByIdentityIdAsync(IdentityId, cancellationToken);

        if (user is not null)
        {
            _cachedUserId = user.Id;
            return user.Id;
        }

        throw new UnauthorizedAccessException("User identification failed.");
    }
}
