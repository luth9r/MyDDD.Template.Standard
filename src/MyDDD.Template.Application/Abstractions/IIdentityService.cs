using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Application.Abstractions;

public interface IIdentityService
{
    Task<Result<AccessTokenResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    UserJwtInfo GetUserJwtInfo(AccessTokenResponse token);

    Task<Result> UpdateUserAttributesAsync(
        string identityId,
        Dictionary<string, string[]> attributes,
        CancellationToken cancellationToken = default);
}
