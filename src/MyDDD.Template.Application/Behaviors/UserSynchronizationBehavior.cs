using MediatR;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Domain;
using MyDDD.Template.Domain.Users;
using Microsoft.Extensions.Logging;

namespace MyDDD.Template.Application.Behaviors;

public sealed partial class UserSynchronizationBehavior<TRequest, TResponse>(
    IUserContext userContext,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<UserSynchronizationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    [LoggerMessage(Level = LogLevel.Information, Message = "User with IdentityId {IdentityId} not found in database. Synchronizing...")]
    private static partial void LogUserNotFound(ILogger logger, string identityId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully synchronized user {UserId} (IdentityId: {IdentityId})")]
    private static partial void LogUserSynchronized(ILogger logger, Guid userId, string identityId);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // We only care about authenticated users
        string identityId;
        try
        {
            identityId = userContext.IdentityId;
        }
        catch (UnauthorizedAccessException)
        {
            return await next(cancellationToken);
        }

        if (string.IsNullOrEmpty(identityId))
        {
            return await next(cancellationToken);
        }

        // Check if user exists in DB
        var user = await userRepository.GetByIdentityIdAsync(identityId, cancellationToken);

        if (user is null)
        {
            LogUserNotFound(logger, identityId);

            // Create user from token claims
            var email = userContext.Email ?? $"{identityId}@placeholder.com";
            var firstName = userContext.FirstName ?? "User";
            var lastName = userContext.LastName ?? string.Empty;

            // If we have a UserId in the token, we MUST use it to match Keycloak's expected userId claim
            var userId = userContext.UserId != Guid.Empty
                ? userContext.UserId
                : Guid.NewGuid();

            var newUser = User.Create(
                userId,
                identityId,
                email,
                firstName,
                lastName);

            userRepository.Add(newUser);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            LogUserSynchronized(logger, newUser.Id, identityId);
        }

        return await next(cancellationToken);
    }
}
