namespace MyDDD.Template.Domain.Primitives;

public interface IValidationResult
{
    public static readonly MyError ValidationMyError = new(
        "ValidationMyError",
        "A validation myError occurred.",
        ErrorType.Validation);

    MyError[] Errors { get; }
}

public sealed class ValidationResult : Result, IValidationResult
{
    private ValidationResult(MyError[] errors)
        : base(false, IValidationResult.ValidationMyError)
    {
        Errors = errors;
    }

    public MyError[] Errors { get; }

    public static ValidationResult WithErrors(MyError[] errors)
    {
        return new ValidationResult(errors);
    }
}

public sealed class ValidationResult<TValue> : Result<TValue>, IValidationResult
{
    private ValidationResult(MyError[] errors)
        : base(default, false, IValidationResult.ValidationMyError)
    {
        Errors = errors;
    }

    public MyError[] Errors { get; }

    public static ValidationResult<TValue> WithErrors(MyError[] errors)
    {
        return new ValidationResult<TValue>(errors);
    }
}
