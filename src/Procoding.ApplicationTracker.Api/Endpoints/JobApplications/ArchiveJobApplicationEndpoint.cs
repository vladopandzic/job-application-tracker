using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.Api.Extensions;
using Procoding.ApplicationTracker.Application.JobApplications.Commands.SetJobApplicationArchived;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;

namespace Procoding.ApplicationTracker.Api.Endpoints.JobApplications;

public class ArchiveJobApplicationEndpoint : EndpointBaseAsync.WithRequest<JobApplicationIdRequestDTO>.WithResult<IActionResult>
{
    private readonly ISender _sender;

    public ArchiveJobApplicationEndpoint(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(AuthenticationSchemes = "BearerEmployee,BearerCandidate", Policy = Policies.CandidateOnly)]
    [HttpPost("job-applications/{JobApplicationId}/archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public override async Task<IActionResult> HandleAsync([FromRoute] JobApplicationIdRequestDTO request, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new SetJobApplicationArchivedCommand(request.JobApplicationId, archived: true), cancellationToken);

        return result.Match<IActionResult>(id => Ok(id), err => BadRequest(err.MapToResponse()));
    }
}
