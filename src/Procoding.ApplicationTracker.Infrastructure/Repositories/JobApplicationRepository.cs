using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.Infrastructure.Data;

namespace Procoding.ApplicationTracker.Infrastructure.Repositories;

internal sealed class JobApplicationRepository : IJobApplicationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public JobApplicationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<JobApplication?> GetJobApplicationAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.JobApplications.Include(x => x.ApplicationSource)
                                               .Include(x => x.Company)
                                               .Include(x => x.Candidate)
                                               .Include(x => x.InterviewSteps)
                                               .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<JobApplication>> GetJobApplicationsAsync(ISpecification<JobApplication> spec, CancellationToken cancellationToken)
    {
        return await _dbContext.JobApplications.ToListAsync(spec, cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<JobApplication> spec, CancellationToken cancellationToken)
    {
        var query = SpecificationEvaluator.Default.GetQuery(_dbContext.JobApplications, spec, true);
        return await query.CountAsync();
    }

    public async Task InsertAsync(JobApplication jobApplication, CancellationToken cancellationToken)
    {
        await _dbContext.JobApplications.AddAsync(jobApplication, cancellationToken);
    }

    public void Delete(JobApplication jobApplication)
    {
        // Soft delete: SaveChangesAsync converts the Remove into setting DeletedOnUtc.
        _dbContext.JobApplications.Remove(jobApplication);
    }
}
