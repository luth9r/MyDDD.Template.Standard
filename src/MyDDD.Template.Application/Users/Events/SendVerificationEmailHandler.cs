using Microsoft.Extensions.Logging;
using MyDDD.Template.Application.Abstractions;

namespace MyDDD.Template.Application.Users.Events;

/// <summary>
/// Handles the <see cref="UserRegisteredIntegrationEvent"/> and sends a verification email.
/// </summary>
public sealed class SendVerificationEmailHandler(
    IIdentityService identityService,
    ILogger<SendVerificationEmailHandler> logger)
{
    public async Task Handle(UserRegisteredIntegrationEvent message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Triggering Keycloak verification email for user {IdentityId} ({Email})", message.IdentityId, message.Email);

        var result = await identityService.SendVerificationEmailAsync(message.IdentityId, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogError("Failed to trigger Keycloak verification email for user {IdentityId}: {Error}", 
                message.IdentityId, result.Error);
            
            // Note: In a production scenario, you might want to retry this or throw an exception 
            // to let Wolverine's retry logic handle it.
            throw new Exception($"Failed to trigger Keycloak verification: {result.Error.Message}");
        }

        logger.LogInformation("Keycloak verification email triggered successfully for user {IdentityId}", message.IdentityId);
    }
}
