using Ateleris.NET.Shared.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ateleris.NET.Shared.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection SetupDataProtectionWithDbPersistance(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<DataProtectionKeyContext>(options =>
        {
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        services
            .AddDataProtection()
            .PersistKeysToDbContext<DataProtectionKeyContext>();

        return services;
    }
}
