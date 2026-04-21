namespace MyDDD.Template.Domain.Primitives;

public interface IValidationResult
{
    public static readonly MyError ValidationMyError = new(
        "ValidationMyError",
        "A validation error occurred.",
        ErrorType.Validation);

    MyError[] Errors { get; }
}
