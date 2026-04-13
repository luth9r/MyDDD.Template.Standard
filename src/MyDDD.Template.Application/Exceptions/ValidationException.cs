namespace MyDDD.Template.Application.Exceptions;
using DomainValidationResult = Domain.Primitives.ValidationResult;

/// <summary>
/// Exception carrying a domain ValidationResult - caught by the API's exception handler.
/// </summary>
public sealed class ValidationException(DomainValidationResult result) : Exception("Validation failed", null)
{
    public DomainValidationResult ValidationResult { get; } = result;

    public override string? StackTrace => null;
}
