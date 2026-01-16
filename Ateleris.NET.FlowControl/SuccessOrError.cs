using System;
using System.Threading.Tasks;

namespace Ateleris.NET.FlowControl;

public class SuccessOrError<E> : ResultOrError<bool, E> where E : Error?
{
    public SuccessOrError() : base(true)
    {
    }

    public SuccessOrError(E error) : base(error)
    {
    }

    public static SuccessOrError<E> Success() => new();

    public new static SuccessOrError<E> Error(E value) => new(value);

    public static implicit operator SuccessOrError<E>(E value) => new(value);

    public void Deconstruct(out E? error)
    {
        error = isError ? this.error : default;
    }
}

public static class SuccessOrErrorExtensions
{
    public static async Task<SuccessOrError<E>> Then<E>(
        this Task<SuccessOrError<E>> first,
        Func<Task<SuccessOrError<E>>> next)
        where E : Error
    {
        var result = await first;
        if (result.isError)
        {
            return result;
        }

        return await next();
    }

    public static async Task<ResultOrError<T, E>> Then<T, E>(
        this SuccessOrError<E> first,
        Func<Task<ResultOrError<T, E>>> next)
        where E : Error
    {
        if (first.isError)
        {
            return ResultOrError<T, E>.Error(first.error!);
        }

        return await next();
    }

    public static async Task<ResultOrError<T, E>> Then<T, E>(
        this Task<SuccessOrError<E>> first,
        Func<Task<ResultOrError<T, E>>> next)
        where E : Error
    {
        var result = await first;
        if (result.isError)
        {
            return ResultOrError<T, E>.Error(result.error!);
        }

        return await next();
    }

    public static ResultOrError<T, E> Then<T, E>(
        this SuccessOrError<E> first,
        Func<ResultOrError<T, E>> next)
        where E : Error
    {
        if (first.isError)
        {
            return ResultOrError<T, E>.Error(first.error!);
        }

        return next();
    }

    public static async Task<ResultOrError<T, E>> Then<T, E>(
        this Task<SuccessOrError<E>> first,
        Func<ResultOrError<T, E>> next)
        where E : Error
    {
        var result = await first;
        if (result.isError)
        {
            return ResultOrError<T, E>.Error(result.error!);
        }

        return next();
    }

    public static async Task<ResultOrError<T, E>> Then<T, E>(
        this SuccessOrError<E> first,
        Func<Task<T>> next)
        where E : Error
    {
        if (first.isError)
        {
            return ResultOrError<T, E>.Error(first.error!);
        }

        var value = await next();
        return ResultOrError<T, E>.Success(value);
    }

    public static async Task<ResultOrError<T, E>> Then<T, E>(
        this Task<SuccessOrError<E>> first,
        Func<Task<T>> next)
        where E : Error
    {
        var result = await first;
        if (result.isError)
        {
            return ResultOrError<T, E>.Error(result.error!);
        }

        var value = await next();
        return ResultOrError<T, E>.Success(value);
    }

    public static ResultOrError<T, E> Then<T, E>(
        this SuccessOrError<E> first,
        Func<T> next)
        where E : Error
    {
        if (first.isError)
        {
            return ResultOrError<T, E>.Error(first.error!);
        }

        return ResultOrError<T, E>.Success(next());
    }

    public static async Task<ResultOrError<T, E>> Then<T, E>(
        this Task<SuccessOrError<E>> first,
        Func<T> next)
        where E : Error
    {
        var result = await first;
        if (result.isError)
        {
            return ResultOrError<T, E>.Error(result.error!);
        }

        return ResultOrError<T, E>.Success(next());
    }

    public static TOut Match<E, TOut>(
        this SuccessOrError<E> result,
        Func<TOut> onSuccess,
        Func<E, TOut> onError)
        where E : Error
    {
        return result.isError ? onError(result.error!) : onSuccess();
    }

    public static async Task<TOut> Match<E, TOut>(
        this SuccessOrError<E> result,
        Func<Task<TOut>> onSuccess,
        Func<E, TOut> onError)
        where E : Error
    {
        return result.isError ? onError(result.error!) : await onSuccess();
    }

    public static async Task<TOut> Match<E, TOut>(
        this SuccessOrError<E> result,
        Func<TOut> onSuccess,
        Func<E, Task<TOut>> onError)
        where E : Error
    {
        return result.isError ? await onError(result.error!) : onSuccess();
    }

    public static async Task<TOut> Match<E, TOut>(
        this SuccessOrError<E> result,
        Func<Task<TOut>> onSuccess,
        Func<E, Task<TOut>> onError)
        where E : Error
    {
        return result.isError ? await onError(result.error!) : await onSuccess();
    }

    public static async Task<TOut> Match<E, TOut>(
        this Task<SuccessOrError<E>> task,
        Func<TOut> onSuccess,
        Func<E, TOut> onError)
        where E : Error
    {
        var result = await task;
        return result.Match(onSuccess, onError);
    }

    public static async Task<TOut> Match<E, TOut>(
        this Task<SuccessOrError<E>> task,
        Func<Task<TOut>> onSuccess,
        Func<E, TOut> onError)
        where E : Error
    {
        var result = await task;
        return await result.Match(onSuccess, onError);
    }

    public static async Task<TOut> Match<E, TOut>(
        this Task<SuccessOrError<E>> task,
        Func<TOut> onSuccess,
        Func<E, Task<TOut>> onError)
        where E : Error
    {
        var result = await task;
        return await result.Match(onSuccess, onError);
    }

    public static async Task<TOut> Match<E, TOut>(
        this Task<SuccessOrError<E>> task,
        Func<Task<TOut>> onSuccess,
        Func<E, Task<TOut>> onError)
        where E : Error
    {
        var result = await task;
        return await result.Match(onSuccess, onError);
    }
}
