using System;

namespace GarageGroup.Infra;

public sealed record class HandlerResultJson<T>
{
    public static HandlerResultJson<T> FromResult(Result<T, Failure<HandlerFailureCode>> result)
        =>
        result.Fold(FromSuccess, FromFailure);

    public static HandlerResultJson<T> FromSuccess(T success)
        =>
        new()
        {
            IsSuccess = true,
            Success = success
        };

    public static HandlerResultJson<T> FromFailure(Failure<HandlerFailureCode> failure)
        =>
        new()
        {
            IsSuccess = false,
            Failure = failure
        };

    public bool IsSuccess { get; init; }

    public T? Success { get; init; }

    public Failure<HandlerFailureCode>? Failure { get; init; }

    public Result<T?, Failure<HandlerFailureCode>> ToResult()
        =>
        IsSuccess ? Result.Success(Success) : Result.Failure(Failure.GetValueOrDefault());

    public static implicit operator HandlerResultJson<T>(T success)
        =>
        FromSuccess(success);

    public static implicit operator HandlerResultJson<T>(Failure<HandlerFailureCode> failure)
        =>
        FromFailure(failure);

    public static implicit operator HandlerResultJson<T>(Result<T, Failure<HandlerFailureCode>> result)
        =>
        FromResult(result);
}