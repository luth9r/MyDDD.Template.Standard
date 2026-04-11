using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Application.Abstractions.Messaging;

public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task Handle(TEvent domainEvent, CancellationToken cancellationToken = default);
}
