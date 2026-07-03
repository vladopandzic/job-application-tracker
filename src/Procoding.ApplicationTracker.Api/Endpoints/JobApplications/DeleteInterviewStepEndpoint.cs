using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.Api.Extensions;
using Procoding.ApplicationTracker.Application.JobApplications.Commands.DeleteInterviewStep;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;

namespace Procoding.ApplicationTracker.Api.Endpoints.JobApplications;

public class DeleteInterviewStepEndpoint : EndpointBaseAsync.WithRequest<DeleteInterviewStepRequestDTO>.WithResult<IActionResult>
{
    private readonly ISender _sender;

    public DeleteInterviewStepEndpoint(ISender sender)
    {
        _sender = sender;
    }

    [HttpDelete("job-applications/{JobApplicationId}/interview-steps/{InterviewStepId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthenticationSchemes = "BearerEmployee,BearerCandidate", Policy = Policies.CandidateOnly)]
    public override async Task<IActionResult> HandleAsync([FromRoute] DeleteInterviewStepRequestDTO request, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new DeleteInterviewStepCommand(jobApplicationId: request.JobApplicationId,
                                                                       interviewStepId: request.InterviewStepId),
                                        cancellationToken);

        return result.Match<IActionResult>(id => Ok(id), err => BadRequest(err.MapToResponse()));
    }
}
