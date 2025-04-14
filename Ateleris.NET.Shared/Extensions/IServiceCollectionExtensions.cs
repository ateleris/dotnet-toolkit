using Ateleris.NET.Shared.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Ateleris.NET.Shared.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection SetupDataProtectionWithDbPersistance(this IServiceCollection services)
    {
        services
            .AddDataProtection()
            .PersistKeysToDbContext<DataProtectionKeyContext>();

        return services;
    }
}
