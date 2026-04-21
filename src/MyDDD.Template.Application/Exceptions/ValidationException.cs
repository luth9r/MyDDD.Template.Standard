namespace MyDDD.Template.Application.Exceptions;

public sealed class ValidationException(Domain.Primitives.ValidationResult result) : Exception("Validation failed", null)
{
    public Domain.Primitives.ValidationResult ValidationResult { get; } = result;

    public override string? StackTrace => null;
}
