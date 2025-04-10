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

    public new bool Value => base.Value;

    public new E? ErrorValue => base.ErrorValue;
}
