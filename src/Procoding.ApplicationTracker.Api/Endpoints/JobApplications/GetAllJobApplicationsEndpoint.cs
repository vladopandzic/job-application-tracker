using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.Application.Core.Query;
using Procoding.ApplicationTracker.Application.JobApplications.Queries.GetAllJobApplications;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Api.Endpoints.JobApplications;

public class GetAllJobApplicationsEndpoint : EndpointBaseAsync.WithRequest<JobApplicationGetListRequestDTO>.WithResult<JobApplicationListResponseDTO>
{
    readonly ISender _sender;
    public GetAllJobApplicationsEndpoint(ISender sender)
    {
        this._sender = sender;
    }

    [HttpGet("job-applications")]
    [Authorize(AuthenticationSchemes = "BearerEmployee,BearerCandidate")]

    public override async Task<JobApplicationListResponseDTO> HandleAsync([FromQuery] JobApplicationGetListRequestDTO request, CancellationToken cancellationToken = default)
    {
        return await _sender.Send(new GetAllJobApplicationsQuery(pageNumber: request.PageNumber,
                                                          pageSize: request.PageSize,
                                                          filters: request.Filters.Select(x => new Filter()
                                                          {
                                                              Key = x.Key,
                                                              Operator = x.Operator,
                                                              Value = x.Value
                                                          }).ToList(),
                                                          sort: request.Sort.Select(x => new Sort()
                                                          {
                                                              SortBy = x.SortBy,
                                                              Descending = x.Descending
                                                          }).ToList(),
                                                          archived: request.Archived), cancellationToken);
    }
}
