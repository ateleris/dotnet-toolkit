using System;

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

    public required TErrorType? Type { get; init; } = default;

    public override string ToString() => $"Error[{Type?.ToString() ?? "-"}]: {Message}";
}
