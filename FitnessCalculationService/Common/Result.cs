using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FitnessCalculationService.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public List<Error> Errors { get; }

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

    public static Result Success() => new(true, (Error)null!);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(List<Error> errors) => new(false, errors);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

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

    public static Result<TValue> Success(TValue value) => new(value, true, (Error)null!);
    public static new Result<TValue> Failure(Error error) => new(default, false, error);
    public static new Result<TValue> Failure(List<Error> errors) => new(default, false, errors);
}

