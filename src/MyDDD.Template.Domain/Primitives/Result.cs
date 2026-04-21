namespace MyDDD.Template.Domain.Primitives;

public class Result
{
    protected Result(bool isSuccess, MyError error)
    {
        if ((isSuccess && error != MyError.None) || (!isSuccess && error == MyError.None))
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public MyError Error { get; }

    public static Result Success()
    {
        return new Result(true, MyError.None);
    }

    public static Result<TValue> Success<TValue>(TValue value)
    {
        return new Result<TValue>(value, true, MyError.None);
    }

    public static Result Failure(MyError myError)
    {
        return new Result(false, myError);
    }

    public static Result<TValue> Failure<TValue>(MyError myError)
    {
        return new Result<TValue>(default, false, myError);
    }
}

public class Result<TValue> : Result
{
    protected internal Result(TValue? value, bool isSuccess, MyError error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public TValue Value => IsSuccess
        ? field!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(MyError.NullValue);
}
