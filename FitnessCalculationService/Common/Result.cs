namespace FitnessCalculationService.Common;

public class Result
{
    protected Result(bool isSuccess, Error error, bool businessRuleFailed = false)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Success result cannot have an error.");

        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error!;
        Errors = error != null ? new List<Error> { error } : new List<Error>();
        BusinessRuleFailed = businessRuleFailed;
    }

    protected Result(bool isSuccess, List<Error> errors, bool businessRuleFailed = false)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? new List<Error>();
        Error = Errors.FirstOrDefault()!;
        BusinessRuleFailed = businessRuleFailed;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public bool BusinessRuleFailed { get; set; } = false;
    public Error Error { get; }
    public List<Error> Errors { get; }

    public static Result Success()
    {
        return new Result(true, (Error)null!);
    }

    public static Result Failure(Error error, bool businessRuleFailed = false)
    {
        return new Result(false, error, businessRuleFailed);
    }

    public static Result Failure(List<Error> errors, bool businessRuleFailed = false)
    {
        return new Result(false, errors, businessRuleFailed);
    }

    public static Result Rollback(Error error)
    {
        return new Result(false, error, businessRuleFailed: true);
    }

    public static Result Rollback(List<Error> errors)
    {
        return new Result(false, errors, businessRuleFailed: true);
    }
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error, bool businessRuleFailed = false)
        : base(isSuccess, error, businessRuleFailed)
    {
        _value = value;
    }

    protected internal Result(TValue? value, bool isSuccess, List<Error> errors, bool businessRuleFailed = false)
        : base(isSuccess, errors, businessRuleFailed)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    public static Result<TValue> Success(TValue value)
    {
        return new Result<TValue>(value, true, (Error)null!);
    }

    public new static Result<TValue> Failure(Error error, bool businessRuleFailed = false)
    {
        return new Result<TValue>(default, false, error, businessRuleFailed);
    }

    public new static Result<TValue> Failure(List<Error> errors, bool businessRuleFailed = false)
    {
        return new Result<TValue>(default, false, errors, businessRuleFailed);
    }

    public new static Result<TValue> Rollback(Error error)
    {
        return new Result<TValue>(default, false, error, businessRuleFailed: true);
    }

    public new static Result<TValue> Rollback(List<Error> errors)
    {
        return new Result<TValue>(default, false, errors, businessRuleFailed: true);
    }
}
