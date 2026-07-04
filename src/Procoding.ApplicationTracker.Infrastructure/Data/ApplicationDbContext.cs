using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Auth;
using Procoding.ApplicationTracker.Domain.Entities;
using System.Reflection;

namespace Procoding.ApplicationTracker.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IUnitOfWork, IDataProtectionKeyContext
{
    private readonly TimeProvider _timeProvider;
    private readonly IIdentityContext _identityContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, TimeProvider timeProvider, IIdentityContext identityContext) : base(options)
    {
        _timeProvider = timeProvider;
        _identityContext = identityContext;
    }

    public DbSet<Candidate> Candidates { get; set; }

    public DbSet<Employee> Employees { get; set; }

    public DbSet<JobApplication> JobApplications { get; set; }

    public DbSet<Company> Companies { get; set; }

    public DbSet<JobApplicationSource> JobApplicationSources { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Translation> Translations { get; set; }

    /// <summary>Data Protection keys — persisted in the DB so auth cookies survive redeploys/recycles.</summary>
    public DbSet<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<JobApplication>().HasQueryFilter(jobApp => _identityContext == null ||
                                                                       _identityContext.IsEmployee == true ||
                                                                       (_identityContext.IsCandidate == true && jobApp.Candidate.Id == _identityContext.UserId));


        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Saves all of the pending changes in the unit of work.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of entities that have been saved.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        DateTime utcNow = _timeProvider.GetUtcNow().DateTime;

        UpdateAuditableEntities(utcNow);

        UpdateSoftDeletableEntities(utcNow);

        //await PublishDomainEvents(cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the entities implementing <see cref="ISoftDeletableEntity"/> interface.
    /// </summary>
    /// <param name="utcNow">The current date and time in UTC format.</param>
    private void UpdateSoftDeletableEntities(DateTime utcNow)
    {
        foreach (EntityEntry<ISoftDeletableEntity> entityEntry in ChangeTracker.Entries<ISoftDeletableEntity>())
        {
            if (entityEntry.State != EntityState.Deleted)
            {
                continue;
            }

            entityEntry.Property(nameof(ISoftDeletableEntity.DeletedOnUtc)).CurrentValue = utcNow;


            entityEntry.State = EntityState.Modified;

            UpdateDeletedEntityEntryReferencesToUnchanged(entityEntry);
        }
    }

    /// <summary>
    /// Updates the specified entity entry's referenced entries in the deleted state to the modified state. This method
    /// is recursive.
    /// </summary>
    /// <param name="entityEntry">The entity entry.</param>
    private static void UpdateDeletedEntityEntryReferencesToUnchanged(EntityEntry entityEntry)
    {
        if (!entityEntry.References.Any())
        {
            return;
        }

        foreach (ReferenceEntry referenceEntry in entityEntry.References.Where(r => r.TargetEntry?.State == EntityState.Deleted))
        {
            if (referenceEntry.TargetEntry is not null)
            {
                referenceEntry.TargetEntry.State = EntityState.Unchanged;

                UpdateDeletedEntityEntryReferencesToUnchanged(referenceEntry.TargetEntry);
            }
        }
    }


    /// <summary>
    /// Updates the entities implementing <see cref="IAuditableEntity"/> interface.
    /// </summary>
    /// <param name="utcNow">The current date and time in UTC format.</param>
    private void UpdateAuditableEntities(DateTime utcNow)
    {
        foreach (EntityEntry<IAuditableEntity> entityEntry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entityEntry.State == EntityState.Added)
            {
                entityEntry.Property(nameof(IAuditableEntity.CreatedOnUtc)).CurrentValue = utcNow;
            }

            if (entityEntry.State == EntityState.Modified)
            {
                entityEntry.Property(nameof(IAuditableEntity.ModifiedOnUtc)).CurrentValue = utcNow;
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder.UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=ApplicationTrackerDb;Trusted_Connection=True;TrustServerCertificate=True;");
        //optionsBuilder.UseNpgsql(@"Host=localhost;Database=ApplicationDbTracker;Username=postgres;Password=admin");
    }
}