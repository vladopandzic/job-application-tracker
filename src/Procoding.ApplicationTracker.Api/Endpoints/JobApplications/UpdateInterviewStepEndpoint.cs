using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.Api.Extensions;
using Procoding.ApplicationTracker.Application.JobApplications.Commands.UpdateInterviewStep;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;

namespace Procoding.ApplicationTracker.Api.Endpoints.JobApplications;

public class UpdateInterviewStepEndpoint : EndpointBaseAsync.WithRequest<UpdateInterviewStepRequestDTO>.WithResult<IActionResult>
{
    private readonly ISender _sender;

    public UpdateInterviewStepEndpoint(ISender sender)
    {
        _sender = sender;
    }

    [HttpPut("job-applications/interview-steps")]
    [ProducesResponseType(typeof(InterviewStepDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthenticationSchemes = "BearerEmployee,BearerCandidate", Policy = Policies.CandidateOnly)]
    public override async Task<IActionResult> HandleAsync(UpdateInterviewStepRequestDTO request, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new UpdateInterviewStepCommand(jobApplicationId: request.JobApplicationId,
                                                                       interviewStepId: request.InterviewStepId,
                                                                       type: request.Type,
                                                                       occurredOn: request.OccurredOn,
                                                                       outcome: request.Outcome,
                                                                       notes: request.Notes),
                                        cancellationToken);

        return result.Match<IActionResult>(dto => Ok(dto), err => BadRequest(err.MapToResponse()));
    }
}
