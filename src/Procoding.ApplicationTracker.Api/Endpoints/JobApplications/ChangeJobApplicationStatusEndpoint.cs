using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.Api.Extensions;
using Procoding.ApplicationTracker.Application.JobApplications.Commands.ChangeJobApplicationStatus;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Api.Endpoints.JobApplications;

public class ChangeJobApplicationStatusEndpoint : EndpointBaseAsync.WithRequest<JobApplicationChangeStatusRequestDTO>.WithResult<IActionResult>
{
    private readonly ISender _sender;

    public ChangeJobApplicationStatusEndpoint(ISender sender)
    {
        _sender = sender;
    }

    [HttpPatch("job-applications/status")]
    [ProducesResponseType(typeof(JobApplicationStatusChangedResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthenticationSchemes = "BearerEmployee,BearerCandidate", Policy = Policies.CandidateOnly)]
    public override async Task<IActionResult> HandleAsync(JobApplicationChangeStatusRequestDTO request, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ChangeJobApplicationStatusCommand(jobApplicationId: request.JobApplicationId,
                                                                              newStatus: request.Status),
                                        cancellationToken);

        return result.Match<IActionResult>(Ok, err => BadRequest(err.MapToResponse()));
    }
}
