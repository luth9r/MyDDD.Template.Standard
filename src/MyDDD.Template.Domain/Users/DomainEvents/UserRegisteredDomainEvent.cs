using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Domain.Users.DomainEvents;

public sealed record UserRegisteredDomainEvent(Guid Id, string IdentityId, string Email) : IDomainEvent;
