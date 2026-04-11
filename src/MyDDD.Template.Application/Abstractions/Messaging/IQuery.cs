using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : MediatR.IRequest<Result<TResponse>>;
