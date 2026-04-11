using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand> : MediatR.IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

public interface ICommandHandler<in TCommand, TResponse> : MediatR.IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
