namespace MyDDD.Template.Domain.Primitives;

public interface IValidationResult
{
    public static readonly MyError ValidationMyError = new(
        "ValidationMyError",
        "A validation error occurred.",
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
