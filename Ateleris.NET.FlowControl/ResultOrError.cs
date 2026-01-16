using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ateleris.NET.FlowControl;

public class ResultOrError<T, E> where E : Error?
{
    internal readonly T? tVal;
    internal readonly E? error;
    internal readonly bool isError;

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

    public Task<U> Match<U>(Func<T, U> f1, Func<E, U> f2)
        => Task.FromResult(!isError ? f1(tVal!) : f2(error!));

    public async Task<U> Match<U>(Func<T, Task<U>> f1, Func<E, U> f2, CancellationToken ct = default)
        => !isError ? await f1(tVal!).WaitAsync(ct) : f2(error!);

    public async Task<U> Match<U>(Func<T, U> f1, Func<E, Task<U>> f2, CancellationToken ct = default)
        => !isError ? f1(tVal!) : await f2(error!).WaitAsync(ct);

    public async Task<U> Match<U>(Func<T, Task<U>> f1, Func<E, Task<U>> f2, CancellationToken ct = default)
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

    public Task Match(Action<T> f1, Action<E> f2)
    {
        if (isError)
        {
            f2(error!);
        }
        else
        {
            f1(tVal!);
        }

        return Task.CompletedTask;
    }

    public Task Match(Func<T, Task> f1, Func<E, Task> f2)
        => !isError ? f1(tVal!) : f2(error!);

    public Task Match(Func<T, Task> f1, Action<E> f2)
    {
        if (isError)
        {
            return f1(tVal!);
        }

        f2(error!);
        return Task.CompletedTask;
    }

    public Task Match(Action<T> f1, Func<E, Task> f2)
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
}

public static class ResultOrErrorExtensions
{
    public static async Task<ResultOrError<TOut, E>> Then<TIn, TOut, E>(
        this Task<ResultOrError<TIn, E>> first,
        Func<TIn, Task<ResultOrError<TOut, E>>> next)
        where E : Error
    {
        var result = await first;
        if (result.isError)
        {
            return ResultOrError<TOut, E>.Error(result.error!);
        }

        return await next(result.tVal!);
    }

    public static async Task<ResultOrError<TOut, E>> Then<TIn, TOut, E>(
        this ResultOrError<TIn, E> first,
        Func<TIn, Task<ResultOrError<TOut, E>>> next)
        where E : Error
    {
        if (first.isError)
        {
            return ResultOrError<TOut, E>.Error(first.error!);
        }

        return await next(first.tVal!);
    }

    public static async Task<ResultOrError<TOut, E>> Then<TIn, TOut, E>(
        this Task<ResultOrError<TIn, E>> first,
        Func<TIn, ResultOrError<TOut, E>> next)
        where E : Error
    {
        var result = await first;
        if (result.isError)
        {
            return ResultOrError<TOut, E>.Error(result.error!);
        }

        return next(result.tVal!);
    }

    public static ResultOrError<TOut, E> Then<TIn, TOut, E>(
        this ResultOrError<TIn, E> first,
        Func<TIn, ResultOrError<TOut, E>> next)
        where E : Error
    {
        if (first.isError)
        {
            return ResultOrError<TOut, E>.Error(first.error!);
        }

        return next(first.tVal!);
    }

    public static async Task<SuccessOrError<E>> Then<T, E>(
        this Task<ResultOrError<T, E>> first,
        Func<T, Task<SuccessOrError<E>>> next)
        where E : Error
    {
        var result = await first;
        if (result.isError)
        {
            return SuccessOrError<E>.Error(result.error!);
        }

        return await next(result.tVal!);
    }

    public static async Task<SuccessOrError<E>> Then<T, E>(
        this Task<ResultOrError<T, E>> first,
        Func<T, SuccessOrError<E>> next)
        where E : Error
    {
        var result = await first;
        if (result.isError)
        {
            return SuccessOrError<E>.Error(result.error!);
        }

        return next(result.tVal!);
    }

    public static async Task<SuccessOrError<E>> Then<T, E>(
        this ResultOrError<T, E> first,
        Func<T, Task<SuccessOrError<E>>> next)
        where E : Error
    {
        if (first.isError)
        {
            return SuccessOrError<E>.Error(first.error!);
        }

        return await next(first.tVal!);
    }

    public static SuccessOrError<E> Then<T, E>(
        this ResultOrError<T, E> first,
        Func<T, SuccessOrError<E>> next)
        where E : Error
    {
        if (first.isError)
        {
            return SuccessOrError<E>.Error(first.error!);
        }

        return next(first.tVal!);
    }

    public static async Task<SuccessOrError<E>> ToSuccessOrError<T, E>(
        this Task<ResultOrError<T, E>> resultTask)
        where E : Error
    {
        var result = await resultTask;
        if (result.isError)
        {
            return SuccessOrError<E>.Error(result.error!);
        }

        return SuccessOrError<E>.Success();
    }

    public static SuccessOrError<E> ToSuccessOrError<T, E>(
        this ResultOrError<T, E> result)
        where E : Error
    {
        if (result.isError)
        {
            return SuccessOrError<E>.Error(result.error!);
        }

        return SuccessOrError<E>.Success();
    }

    public static async Task<TOut> Match<T, E, TOut>(
        this Task<ResultOrError<T, E>> task,
        Func<T, TOut> onSuccess,
        Func<E, TOut> onError)
        where E : Error
    {
        var result = await task;
        return !result.isError ? onSuccess(result.tVal!) : onError(result.error!);
    }

    public static async Task<TOut> Match<T, E, TOut>(
        this Task<ResultOrError<T, E>> task,
        Func<T, Task<TOut>> onSuccess,
        Func<E, TOut> onError)
        where E : Error
    {
        var result = await task;
        return !result.isError ? await onSuccess(result.tVal!) : onError(result.error!);
    }

    public static async Task<TOut> Match<T, E, TOut>(
        this Task<ResultOrError<T, E>> task,
        Func<T, TOut> onSuccess,
        Func<E, Task<TOut>> onError)
        where E : Error
    {
        var result = await task;
        return !result.isError ? onSuccess(result.tVal!) : await onError(result.error!);
    }

    public static async Task<TOut> Match<T, E, TOut>(
        this Task<ResultOrError<T, E>> task,
        Func<T, Task<TOut>> onSuccess,
        Func<E, Task<TOut>> onError)
        where E : Error
    {
        var result = await task;
        return !result.isError ? await onSuccess(result.tVal!) : await onError(result.error!);
    }
}
