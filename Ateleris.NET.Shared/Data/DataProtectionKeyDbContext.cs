using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ateleris.NET.Shared.Data;

public class DataProtectionKeyContext(DbContextOptions<DataProtectionKeyContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public required DbSet<DataProtectionKey> DataProtectionKeys { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("net_data_protection");
        modelBuilder.Entity<DataProtectionKey>().ToTable("data_protection_keys");
    }
}
