using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.Api.Extensions;
using Procoding.ApplicationTracker.Application.JobApplications.Commands.ExtractJobPostingFromText;
using Procoding.ApplicationTracker.Domain.Exceptions;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Api.Endpoints.JobApplications;

/// <summary>
/// AI-assisted import: takes pasted job posting text and returns structured fields to pre-fill the
/// new-application form. Candidate-only — must be signed in (also caps abuse of the AI quota).
/// </summary>
public class ExtractJobPostingEndpoint : EndpointBaseAsync.WithRequest<ExtractJobPostingRequestDTO>.WithResult<IActionResult>
{
    private readonly ISender _sender;

    public ExtractJobPostingEndpoint(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(AuthenticationSchemes = "BearerEmployee,BearerCandidate", Policy = Policies.CandidateOnly)]
    [HttpPost("job-applications/extract-from-text")]
    [ProducesResponseType(typeof(ExtractedJobPostingResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public override async Task<IActionResult> HandleAsync([FromBody] ExtractJobPostingRequestDTO request,
                                                          CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ExtractJobPostingFromTextCommand(request.Content), cancellationToken);

        return result.Match<IActionResult>(Ok, err => err is Unauthorized401Exception ? Unauthorized(err.MapToResponse()) : BadRequest(err.MapToResponse()));
    }
}
