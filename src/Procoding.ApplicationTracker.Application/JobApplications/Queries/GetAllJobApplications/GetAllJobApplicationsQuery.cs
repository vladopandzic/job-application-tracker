using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Application.Core.Query;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Application.JobApplications.Queries.GetAllJobApplications;

public sealed class GetAllJobApplicationsQuery : IQuery<JobApplicationListResponseDTO>
{
    public GetAllJobApplicationsQuery(int? pageNumber, int? pageSize, List<Filter> filters, List<Sort> sort, bool archived = false)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Filters = filters;
        Sort = sort;
        Archived = archived;
    }

    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }

    public List<Filter> Filters { get; set; }

    public List<Sort> Sort { get; set; }

    public bool Archived { get; set; }
}
