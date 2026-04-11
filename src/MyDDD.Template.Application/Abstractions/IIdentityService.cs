using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Application.Abstractions;

public interface IIdentityService
{
    Task<Result<string>> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        Guid internalUserId,
        CancellationToken cancellationToken = default);

    Task<Result<AccessTokenResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<Result> SendVerificationEmailAsync(string identityId, CancellationToken cancellationToken = default);
}
