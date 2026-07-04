using Ardalis.Specification;
using Procoding.ApplicationTracker.Domain.Entities;

namespace Procoding.ApplicationTracker.Domain.Repositories;

public interface IJobApplicationRepository
{
    /// <summary>
    /// Gets total number that satisfy <paramref name="spec"/>.
    /// </summary>
    /// <param name="spec"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> CountAsync(ISpecification<JobApplication> spec, CancellationToken cancellationToken);

    /// <summary>
    /// Gets job applications.
    /// </summary>
    /// <param name="spec"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<JobApplication>> GetJobApplicationsAsync(ISpecification<JobApplication> spec, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts the specified jobApplication to the database.
    /// </summary>
    /// <param name="jobApplication">The job application to be inserted to the database.</param>
    Task InsertAsync(JobApplication jobApplication, CancellationToken cancellationToken);


    /// <summary>
    /// Get one job application.
    /// </summary>
    /// <param name="obApplicationId"></param>
    /// <returns></returns>
    Task<JobApplication?> GetJobApplicationAsync(Guid obApplicationId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the job application (soft delete — sets DeletedOnUtc on save).
    /// </summary>
    void Delete(JobApplication jobApplication);
}
