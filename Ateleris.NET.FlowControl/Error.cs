using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ateleris.NET.FlowControl;

public class Error
{
    public static Error New(string message) => new()
    {
        Message = message
    };

    public static Error New(Exception ex) => new()
    {
        Message = ex.Message
    };

    public required string Message { get; init; }

    public override string ToString() => $"Error: {Message}";
}

public class Error<TErrorType> : Error
{
    public ImmutableList<Error<TErrorType>>? ChildErrors { get; init; }

    public static Error<TErrorType> New(string message, TErrorType? type = default) => new()
    {
        Message = message,
        Type = type
    };

    public static Error<TErrorType> New(TErrorType? type) => new()
    {
        Message = string.Empty,
        Type = type
    };

    public static Error<TErrorType> New(Exception ex, TErrorType? type = default) => new()
    {
        Message = ex.Message,
        Type = type
    };

    public static Error<TErrorType> Composite(string message, TErrorType type, params Error<TErrorType>[] errors) => new()
    {
        Message = message,
        Type = type,
        ChildErrors = [.. errors]
    };

    public static Error<TErrorType> Composite(string message, TErrorType type, IEnumerable<Error<TErrorType>> errors) => new()
    {
        Message = message,
        Type = type,
        ChildErrors = [.. errors]
    };

    public bool IsComposite => ChildErrors is not null && ChildErrors.Count > 0;

    public required TErrorType? Type { get; init; } = default;

    public override string ToString()
    {
        if (!IsComposite)
        {
            return $"Error[{Type?.ToString() ?? "-"}]: {Message}";
        }

        return $"CompositeError[{Type?.ToString() ?? "-"}]: {Message}\n" +
               string.Join("\n", ChildErrors!.Select((e, i) => $"  {i + 1}. {e}"));
    }
}
