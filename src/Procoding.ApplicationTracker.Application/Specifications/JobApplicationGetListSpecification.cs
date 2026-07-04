using Ardalis.Specification;
using Procoding.ApplicationTracker.Application.Core.Query;
using Procoding.ApplicationTracker.Domain.Entities;

namespace Procoding.ApplicationTracker.Application.Specifications;

internal class JobApplicationGetListSpecification : Specification<JobApplication>
{
    public JobApplicationGetListSpecification(int? pageNumber, int? pageSize, List<Filter> filters, List<Sort> sort, bool onlyArchived = false)
    {
        // Active board/list show non-archived; the archive view shows archived. Applied first so it
        // always holds regardless of the dynamic user filters.
        if (onlyArchived)
        {
            Query.Where(x => x.ArchivedOnUtc != null);
        }
        else
        {
            Query.Where(x => x.ArchivedOnUtc == null);
        }

        Query.ApplyPaging(pageNumber, pageSize);

        Query.ApplyFilters(filters.ToList());

        Query.ApplySorting(sort.ToList());

        Query.Include(x => x.Candidate);

        Query.Include(x => x.ApplicationSource);

        Query.Include(x => x.Company);

        Query.Include(x => x.InterviewSteps);
    }
}
