using System;
using System.Threading.Tasks;

namespace Ateleris.NET.FlowControl;

public class SuccessOrError<E> : ResultOrError<bool, E> where E : Error
{
    public SuccessOrError() : base(true)
    {
    }

    public SuccessOrError(E error) : base(error)
    {
    }

    public static SuccessOrError<E> Success()
    {
        return new SuccessOrError<E>();
    }

    public new static SuccessOrError<E> Error(E value) => new(value);

    public static implicit operator SuccessOrError<E>(E value)
    {
        return new SuccessOrError<E>(value);
    }

    public async Task<SuccessOrError<E>> Then(Func<Task<SuccessOrError<E>>> next)
    {
        if (IsError)
            return this;

        return await next();
    }

    public static async Task<SuccessOrError<E>> Then(Task<SuccessOrError<E>> first, Func<Task<SuccessOrError<E>>> next)
    {
        var result = await first;
        if (result.IsError)
            return result;

        return await next();
    }

    public new bool Value => base.Value;

    public new E? ErrorValue => base.ErrorValue;
}

public static class SuccessOrErrorExtensions
{
    public static async Task<SuccessOrError<E>> Then<E>(
        this Task<SuccessOrError<E>> first,
        Func<Task<SuccessOrError<E>>> next)
        where E : Error
    {
        var result = await first;
        if (result.IsError)
            return result;

        return await next();
    }
}
