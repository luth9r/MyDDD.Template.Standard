using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : MediatR.IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
