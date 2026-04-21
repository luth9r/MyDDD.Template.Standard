using FluentValidation.Results;
using MyDDD.Template.Application.Exceptions;
using MyDDD.Template.Domain.Primitives;
using Wolverine.FluentValidation;
using DomainValidationResult = MyDDD.Template.Domain.Primitives.ValidationResult;

namespace MyDDD.Template.Infrastructure.Behaviors;

public class ValidationFailureAction<T> : IFailureAction<T>
{
    public void Throw(T message, IReadOnlyList<ValidationFailure> failures)
    {
        var errors = failures
            .Select(f => MyError.Validation(f.PropertyName, f.ErrorMessage))
            .Distinct()
            .ToArray();

        throw new ValidationException(DomainValidationResult.WithErrors(errors));
    }
}
