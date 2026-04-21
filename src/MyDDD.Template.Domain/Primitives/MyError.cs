namespace MyDDD.Template.Domain.Primitives;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Problem = 4,
    Unauthorised = 5,
}

public sealed record MyError(string Code, string Message, ErrorType Type)
{
    public static readonly MyError None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly MyError NullValue = new("Error.NullValue", "Null value was provided", ErrorType.Failure);

    public static MyError Failure(string code, string message)
    {
        return new MyError(code, message, ErrorType.Failure);
    }

    public static MyError NotFound(string code, string message)
    {
        return new MyError(code, message, ErrorType.NotFound);
    }

    public static MyError Problem(string code, string message)
    {
        return new MyError(code, message, ErrorType.Problem);
    }

    public static MyError Conflict(string code, string message)
    {
        return new MyError(code, message, ErrorType.Conflict);
    }

    public static MyError Validation(string code, string message)
    {
        return new MyError(code, message, ErrorType.Validation);
    }

    public static MyError Unauthorised(string code, string message)
    {
        return new MyError(code, message, ErrorType.Unauthorised);
    }
}
