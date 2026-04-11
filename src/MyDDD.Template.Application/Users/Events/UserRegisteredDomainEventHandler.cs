using MyDDD.Template.Domain.Users.DomainEvents;
using Wolverine;

namespace MyDDD.Template.Application.Users.Events;

/// <summary>
/// Handles the <see cref="UserRegisteredDomainEvent"/> and publishes a <see cref="UserRegisteredIntegrationEvent"/>.
/// </summary>
public sealed class UserRegisteredDomainEventHandler
{
    public async Task Handle(
        UserRegisteredDomainEvent notification,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"[Debug] UserRegisteredDomainEventHandler called! IdentityId={notification.IdentityId}");

        // Map the domain event to an integration event
        var integrationEvent =
            new UserRegisteredIntegrationEvent(notification.IdentityId, notification.Email);

        // Publish via Wolverine (the Outbox is handled automatically if configured)
        await bus.PublishAsync(integrationEvent);
    }
}
