using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Procoding.ApplicationTracker.Infrastructure.Data;

/// <summary>
/// Minimal context that maps ONLY the Data Protection keys table. Used by the Web host (which isn't the
/// composition root for the full <see cref="ApplicationDbContext"/>) to share the same DB-persisted key
/// ring, so its auth cookie survives redeploys. Maps to the same "DataProtectionKeys" table that the API's
/// migration creates.
/// </summary>
public class DataProtectionKeysDbContext : DbContext, IDataProtectionKeyContext
{
    public DataProtectionKeysDbContext(DbContextOptions<DataProtectionKeysDbContext> options) : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}
