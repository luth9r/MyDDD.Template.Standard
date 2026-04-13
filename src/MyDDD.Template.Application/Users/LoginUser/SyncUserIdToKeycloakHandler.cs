using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Application.Exceptions;

namespace MyDDD.Template.Application.Users.LoginUser;

public static class SyncUserIdToKeycloakHandler
{
    public static async Task Handle(
        SyncUserIdToKeycloak message,
        IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        var result = await identityService.UpdateUserAttributesAsync(
            message.IdentityId,
            new Dictionary<string, string[]> { ["userId"] = [message.UserId.ToString()] },
            cancellationToken);

        if (result.IsFailure)
        {
            throw new IdentitySyncException($"Failed to sync: {result.Error.Message}");
        }
    }
}
