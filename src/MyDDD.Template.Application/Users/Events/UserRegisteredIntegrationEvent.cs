using MyDDD.Template.Application.Abstractions.Messaging;

namespace MyDDD.Template.Application.Users.Events;

/// <summary>
/// Integration event published when a new user is registered.
/// </summary>
public sealed record UserRegisteredIntegrationEvent(string IdentityId, string Email) : IIntegrationEvent;
