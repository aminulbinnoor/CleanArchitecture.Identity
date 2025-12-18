using System;

namespace Identity.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
    public static Result<T> Failure<T>(string error) => new(default!, false, error);
}

public class Result<T> : Result
{
    private readonly T _value;

    public T Value => IsSuccess 
        ? _value 
        : throw new InvalidOperationException("Cannot access value of a failed result");

    protected internal Result(T value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        _value = value;
    }
}