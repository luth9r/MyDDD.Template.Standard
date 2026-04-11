namespace MyDDD.Template.Domain.Primitives;

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided", ErrorType.Failure);

    public static Error Failure(string code, string message)
    {
        return new Error(code, message, ErrorType.Failure);
    }

    public static Error NotFound(string code, string message)
    {
        return new Error(code, message, ErrorType.NotFound);
    }

    public static Error Problem(string code, string message)
    {
        return new Error(code, message, ErrorType.Problem);
    }

    public static Error Conflict(string code, string message)
    {
        return new Error(code, message, ErrorType.Conflict);
    }

    public static Error Validation(string code, string message)
    {
        return new Error(code, message, ErrorType.Validation);
    }

    public static Error Unauthorised(string code, string message)
    {
        return new Error(code, message, ErrorType.Unauthorised);
    }
}

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Problem = 4,
    Unauthorised = 5,
}
