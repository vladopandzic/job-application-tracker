using Microsoft.EntityFrameworkCore;
using Procoding.ApplicationTracker.Domain.Entities;

namespace Procoding.ApplicationTracker.Infrastructure.Data;

/// <summary>
/// Seeds the common lookup data the UI needs to be usable out of the box — currently the popular job
/// application sources so the "source" dropdown is never empty. Idempotent: only inserts names that
/// don't already exist, so it's safe to run on every startup and won't disturb admin-added sources.
/// </summary>
public static class ReferenceDataSeeder
{
    private static readonly string[] SourceNames =
    {
        "LinkedIn", "Indeed", "Glassdoor", "Web stranica tvrtke", "Preporuka", "Recruiter", "MojPosao", "Ostalo"
    };

    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        var existing = await db.JobApplicationSources
                               .IgnoreQueryFilters()
                               .Select(s => s.Name)
                               .ToListAsync(cancellationToken);

        var toAdd = SourceNames
            .Where(name => !existing.Contains(name, StringComparer.OrdinalIgnoreCase))
            .Select(name => JobApplicationSource.Create(Guid.NewGuid(), name))
            .ToList();

        if (toAdd.Count > 0)
        {
            await db.JobApplicationSources.AddRangeAsync(toAdd, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
