namespace SubscriptionBillingService.Common;

public class Result
{
    protected Result(bool isSuccess)
    {
        IsSuccess = isSuccess;
        Error = default!;
        Errors = new List<Error>();
        BusinessRuleFailed = false;
    }

    protected Result(bool isSuccess, Error error, bool businessRuleFailed = false)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Success result cannot have an error.");

        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error!;
        Errors = new List<Error> { error! };
        BusinessRuleFailed = businessRuleFailed;
    }

    protected Result(bool isSuccess, List<Error> errors, bool businessRuleFailed = false)
    {
        IsSuccess = isSuccess;
        Errors = errors;
        Error = errors.FirstOrDefault()!;
        BusinessRuleFailed = businessRuleFailed;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public bool BusinessRuleFailed { get; set; }
    public Error Error { get; }
    public List<Error> Errors { get; }

    public static Result Success() => new(true);
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result Failure(Error error, bool businessRuleFailed = false) => new(false, error, businessRuleFailed);
    public static Result<T> Failure<T>(Error error, bool businessRuleFailed = false) => Result<T>.Failure(error, businessRuleFailed);
    public static Result Failure(List<Error> errors, bool businessRuleFailed = false) => new(false, errors, businessRuleFailed);
}

public class Result<T> : Result
{
    private readonly T? _value;

    protected Result(bool isSuccess, T? value, Error error, bool businessRuleFailed = false)
        : base(isSuccess, error, businessRuleFailed)
    {
        _value = value;
    }

    protected Result(bool isSuccess, T? value)
        : base(isSuccess)
    {
        _value = value;
    }

    protected Result(bool isSuccess, List<Error> errors, bool businessRuleFailed = false)
        : base(isSuccess, errors, businessRuleFailed)
    {
        _value = default;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    public static Result<T> Success(T value) => new(true, value);
    public static new Result<T> Failure(Error error, bool businessRuleFailed = false) => new(false, default, error, businessRuleFailed);
    public static new Result<T> Failure(List<Error> errors, bool businessRuleFailed = false) => new(false, errors, businessRuleFailed);
}
