namespace MyDDD.Template.Domain.Primitives;

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
