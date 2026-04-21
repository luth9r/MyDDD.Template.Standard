using FluentValidation;
using Microsoft.Extensions.Logging;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Domain.Users;

namespace MyDDD.Template.Application.Users.LoginUser;

public record LoginResponse(AccessTokenResponse Token, Guid UserId);

public record LoginUserCommand(string Email, string Password);

[Wolverine.Attributes.Transactional]
public static partial class LoginUserCommandHandler
{
    public static async Task<(Result<LoginResponse> Result, SyncUserIdToKeycloak? SyncMessage)> Handle(
        LoginUserCommand request,
        IIdentityService identityService,
        IUserRepository userRepository,
        ILogger<LoginUserCommand> logger,
        CancellationToken cancellationToken)
    {
        var loginResult = await identityService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (loginResult.IsFailure)
        {
            LogLoginFailed(logger, request.Email, loginResult.Error.Message);
            return (Result.Failure<LoginResponse>(loginResult.Error), null);
        }

        var userInfo = identityService.GetUserJwtInfo(loginResult.Value);

        if (!userInfo.IsEmailVerified)
        {
            LogEmailNotVerified(logger, userInfo.IdentityId);
            return (Result.Failure<LoginResponse>(
                MyError.Validation("Auth.EmailNotVerified", "Please verify your email first")), null);
        }

        var (userId, needsSync) = await EnsureUserExists(userInfo, userRepository, logger, cancellationToken);

        var syncMessage = needsSync
            ? new SyncUserIdToKeycloak(userInfo.IdentityId, userId)
            : null;

        var response = new LoginResponse(loginResult.Value, userId);

        return (Result.Success(response), syncMessage);
    }

    private static async Task<(Guid UserId, bool NeedsSync)> EnsureUserExists(
        UserJwtInfo userInfo,
        IUserRepository userRepository,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByIdentityIdAsync(userInfo.IdentityId, cancellationToken);
        if (existingUser is not null)
        {
            // User already exists in database, but it's not linked to Keycloak yet
            if (!userInfo.InternalUserId.HasValue || userInfo.InternalUserId.Value != existingUser.Id)
            {
                LogMissingKeycloakId(logger, existingUser.IdentityId, existingUser.Id);
                return (existingUser.Id, true);
            }

            // Synced
            return (existingUser.Id, false);
        }

        var existingByEmail = await userRepository.GetByEmailAsync(userInfo.Email, cancellationToken);
        if (existingByEmail is not null)
        {
            existingByEmail.UpdateIdentityId(userInfo.IdentityId);

            LogIdentityIdUpdated(logger, userInfo.Email, userInfo.IdentityId);

            return await SyncExistingUser(existingByEmail, userInfo, logger);
        }

        if (userInfo.InternalUserId.HasValue)
        {
            LogGhostIdDetected(logger, userInfo.IdentityId, userInfo.InternalUserId.Value);

            var ghostUser = User.Create(
                userInfo.IdentityId,
                userInfo.Email,
                userInfo.FirstName,
                userInfo.LastName,
                userInfo.InternalUserId.Value);

            userRepository.Add(ghostUser);
            return (ghostUser.Id, false);
        }

        var newUser = User.Create(
            userInfo.IdentityId,
            userInfo.Email,
            userInfo.FirstName,
            userInfo.LastName);

        userRepository.Add(newUser);
        LogNewUserCreated(logger, newUser.IdentityId, newUser.Id);

        return (newUser.Id, true);
    }

    private static ValueTask<(Guid UserId, bool NeedsSync)> SyncExistingUser(
        User user,
        UserJwtInfo userInfo,
        ILogger logger)
    {
        if (!userInfo.InternalUserId.HasValue || userInfo.InternalUserId.Value != user.Id)
        {
            LogMissingKeycloakId(logger, user.IdentityId, user.Id);
            return ValueTask.FromResult((user.Id, true));
        }

        return ValueTask.FromResult((user.Id, false));
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Login failed for email {Email}. Reason: {Error}")]
    private static partial void LogLoginFailed(ILogger logger, string email, string error);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Login blocked. Email not verified for IdentityId: {IdentityId}")]
    private static partial void LogEmailNotVerified(ILogger logger, string identityId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message =
            "Ghost user detected for IdentityId: {IdentityId}. Recreating in DB with existing UserId: {OldUserId}")]
    private static partial void LogGhostIdDetected(ILogger logger, string identityId, Guid oldUserId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "User {IdentityId} exists in DB ({UserId}) but missing in Keycloak token. Queuing sync.")]
    private static partial void LogMissingKeycloakId(ILogger logger, string identityId, Guid userId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "New user created in DB: {UserId} (IdentityId: {IdentityId}). Queuing sync to Keycloak.")]
    private static partial void LogNewUserCreated(ILogger logger, string identityId, Guid userId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "User with email {Email} found by email lookup. Updating IdentityId to {NewIdentityId}")]
    private static partial void LogIdentityIdUpdated(ILogger logger, string email, string newIdentityId);
}

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
