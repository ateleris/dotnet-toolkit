using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ateleris.NET.FlowControl;

public class ResultOrError<T, E> where E : Error?
{
    private readonly T? tVal;
    private readonly E? error;
    private readonly bool isError;

    public ResultOrError(T value)
    {
        tVal = value;
        isError = false;
    }

    public ResultOrError(E value)
    {
        error = value;
        isError = true;
    }

    public static implicit operator ResultOrError<T, E>(T value)
    {
        return new ResultOrError<T, E>(value);
    }

    public static implicit operator ResultOrError<T, E>(E value)
    {
        return new ResultOrError<T, E>(value);
    }

    public void Deconstruct(out T? value, out E? error)
    {
        value = isError ? default : tVal;
        error = isError ? this.error : default;
    }

    public Task<U> MatchFuncAsync<U>(Func<T, U> f1, Func<E, U> f2)
        => Task.FromResult(!isError ? f1(tVal!) : f2(error!));

    public async Task<U> MatchFuncAsync<U>(Func<T, Task<U>> f1, Func<E, U> f2, CancellationToken ct = default)
        => !isError ? await f1(tVal!).WaitAsync(ct) : f2(error!);

    public async Task<U> MatchFuncAsync<U>(Func<T, U> f1, Func<E, Task<U>> f2, CancellationToken ct = default)
        => !isError ? f1(tVal!) : await f2(error!).WaitAsync(ct);

    public async Task<U> MatchFuncAsync<U>(Func<T, Task<U>> f1, Func<E, Task<U>> f2, CancellationToken ct = default)
        => !isError ? await f1(tVal!).WaitAsync(ct) : await f2(error!).WaitAsync(ct);

    public void MatchAction(Action<T> f1, Action<E> f2)
    {
        if (!isError)
        {
            f1(tVal!);
        }
        else
        {
            f2(error!);
        }
    }

    public Task MatchActionAsync(Action<T> f1, Action<E> f2)
    {
        if (isError) f2(error!);
        else f1(tVal!);
        return Task.CompletedTask;
    }

    public Task MatchActionAsync(Func<T, Task> f1, Func<E, Task> f2)
        => !isError ? f1(tVal!) : f2(error!);

    public Task MatchActionAsync(Func<T, Task> f1, Action<E> f2)
    {
        if (isError) return f1(tVal!);
        f2(error!);
        return Task.CompletedTask;
    }

    public Task MatchActionAsync(Action<T> f1, Func<E, Task> f2)
    {
        if (isError)
        {
            f1(tVal!);
            return Task.CompletedTask;
        }
        return f2(error!);
    }

    public static ResultOrError<T, E> Success(T value)
    {
        return new ResultOrError<T, E>(value);
    }

    public static ResultOrError<T, E> Error(E value)
    {
        return new ResultOrError<T, E>(value);
    }

    public bool IsSuccess => !isError;

    public bool IsError => isError;

    public T Value => !isError ? tVal! : throw new InvalidOperationException("Cannot access Value on an Error result.");

    public E ErrorValue => isError ? error! : throw new InvalidOperationException("Cannot access ErrorValue on a Success result.");
}

public static class ResultOrErrorExtensions
{
    // Then for Task<ResultOrError<T, E>> -> passes T to next function
    public static async Task<ResultOrError<TOut, E>> Then<TIn, TOut, E>(
        this Task<ResultOrError<TIn, E>> first,
        Func<TIn, Task<ResultOrError<TOut, E>>> next)
        where E : Error
    {
        var result = await first;
        if (result.IsError)
            return ResultOrError<TOut, E>.Error(result.ErrorValue);

        return await next(result.Value);
    }

    // Then for ResultOrError<T, E> -> passes T to next function
    public static async Task<ResultOrError<TOut, E>> Then<TIn, TOut, E>(
        this ResultOrError<TIn, E> first,
        Func<TIn, Task<ResultOrError<TOut, E>>> next)
        where E : Error
    {
        if (first.IsError)
            return ResultOrError<TOut, E>.Error(first.ErrorValue);

        return await next(first.Value);
    }

    // Then for Task<ResultOrError<T, E>> with synchronous next function
    public static async Task<ResultOrError<TOut, E>> Then<TIn, TOut, E>(
        this Task<ResultOrError<TIn, E>> first,
        Func<TIn, ResultOrError<TOut, E>> next)
        where E : Error
    {
        var result = await first;
        if (result.IsError)
            return ResultOrError<TOut, E>.Error(result.ErrorValue);

        return next(result.Value);
    }

    // Then for ResultOrError<T, E> with synchronous next function
    public static ResultOrError<TOut, E> Then<TIn, TOut, E>(
        this ResultOrError<TIn, E> first,
        Func<TIn, ResultOrError<TOut, E>> next)
        where E : Error
    {
        if (first.IsError)
            return ResultOrError<TOut, E>.Error(first.ErrorValue);

        return next(first.Value);
    }
}
