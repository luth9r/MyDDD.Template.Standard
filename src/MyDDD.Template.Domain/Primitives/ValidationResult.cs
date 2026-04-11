namespace MyDDD.Template.Domain.Primitives;

public interface IValidationResult
{
    public static readonly Error ValidationError = new(
        "ValidationError",
        "A validation error occurred.",
        ErrorType.Validation);

    Error[] Errors { get; }
}

public sealed class ValidationResult : Result, IValidationResult
{
    private ValidationResult(Error[] errors)
        : base(false, IValidationResult.ValidationError)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    public static ValidationResult WithErrors(Error[] errors)
    {
        return new ValidationResult(errors);
    }
}

public sealed class ValidationResult<TValue> : Result<TValue>, IValidationResult
{
    private ValidationResult(Error[] errors)
        : base(default, false, IValidationResult.ValidationError)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    public static ValidationResult<TValue> WithErrors(Error[] errors)
    {
        return new ValidationResult<TValue>(errors);
    }
}
