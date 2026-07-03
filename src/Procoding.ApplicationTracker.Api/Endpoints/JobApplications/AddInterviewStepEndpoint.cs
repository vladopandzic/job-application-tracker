using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.Api.Extensions;
using Procoding.ApplicationTracker.Application.JobApplications.Commands.AddInterviewStep;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Api.Endpoints.JobApplications;

public class AddInterviewStepEndpoint : EndpointBaseAsync.WithRequest<AddInterviewStepRequestDTO>.WithResult<IActionResult>
{
    private readonly ISender _sender;

    public AddInterviewStepEndpoint(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("job-applications/interview-steps")]
    [ProducesResponseType(typeof(InterviewStepAddedResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthenticationSchemes = "BearerEmployee,BearerCandidate", Policy = Policies.CandidateOnly)]
    public override async Task<IActionResult> HandleAsync(AddInterviewStepRequestDTO request, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new AddInterviewStepCommand(jobApplicationId: request.JobApplicationId,
                                                                    type: request.Type,
                                                                    occurredOn: request.OccurredOn,
                                                                    outcome: request.Outcome,
                                                                    notes: request.Notes),
                                        cancellationToken);

        return result.Match<IActionResult>(Ok, err => BadRequest(err.MapToResponse()));
    }
}
