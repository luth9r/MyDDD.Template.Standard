namespace MyDDD.Template.Application.Exceptions;

using DomainValidationResult = Domain.Primitives.ValidationResult;

public sealed class ValidationException(DomainValidationResult result) : Exception("Validation failed", null)
{
    public DomainValidationResult ValidationResult { get; } = result;

    public override string? StackTrace => null;
}
