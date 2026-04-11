using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Domain.Projects.DomainEvents;

public sealed record ProjectCreatedDomainEvent(Guid Id, Guid UserId) : IDomainEvent;
