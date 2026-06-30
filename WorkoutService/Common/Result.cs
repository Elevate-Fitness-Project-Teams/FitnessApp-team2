namespace WorkoutService.Common;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException();

        if (!isSuccess && error == null)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error!;
        Errors = new List<Error> { error! };
    }

    protected Result(bool isSuccess, List<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
        Error = errors.FirstOrDefault()!;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public List<Error> Errors { get; }

    public static Result Success()
    {
        return new Result(true, (Error)null!);
    }

    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    public static Result Failure(List<Error> errors)
    {
        return new Result(false, errors);
    }
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    protected internal Result(TValue? value, bool isSuccess, List<Error> errors)
        : base(isSuccess, errors)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static Result<TValue> Success(TValue value)
    {
        return new Result<TValue>(value, true, (Error)null!);
    }

    public new static Result<TValue> Failure(Error error)
    {
        return new Result<TValue>(default, false, error);
    }

    public new static Result<TValue> Failure(List<Error> errors)
    {
        return new Result<TValue>(default, false, errors);
    }
}
