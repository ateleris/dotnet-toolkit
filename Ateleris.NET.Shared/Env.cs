using System;

namespace Ateleris.NET.Shared;

public static class Env
{
    public static readonly string? Value;

    static Env()
    {
        Value = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        switch (Value)
        {
            case "Production":
                IsProduction = true;
                break;
            case "Staging":
                IsStaging = true;
                break;
            case "Development":
                IsDevelopment = true;
                break;
            default:
                throw new Exception("Unkonwn value for 'ASPNETCORE_ENVIRONMENT' environment variable!");
        }
    }

    public static bool IsProduction { get; private set; }
    public static bool IsStaging { get; private set; }
    public static bool IsDevelopment { get; private set; }
}
