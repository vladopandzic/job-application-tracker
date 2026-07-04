using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.Api.Extensions;
using Procoding.ApplicationTracker.Application.JobApplications.Commands.ExtractJobPostingFromUrl;
using Procoding.ApplicationTracker.Domain.Exceptions;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Api.Endpoints.JobApplications;

/// <summary>
/// AI-assisted import from a URL: fetches the posting page, then extracts structured fields.
/// Candidate-only. Best-effort — some sites block bots, in which case the user pastes text instead.
/// </summary>
public class ExtractJobPostingFromUrlEndpoint : EndpointBaseAsync.WithRequest<ExtractJobPostingFromUrlRequestDTO>.WithResult<IActionResult>
{
    private readonly ISender _sender;

    public ExtractJobPostingFromUrlEndpoint(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(AuthenticationSchemes = "BearerEmployee,BearerCandidate", Policy = Policies.CandidateOnly)]
    [HttpPost("job-applications/extract-from-url")]
    [ProducesResponseType(typeof(ExtractedJobPostingResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public override async Task<IActionResult> HandleAsync([FromBody] ExtractJobPostingFromUrlRequestDTO request,
                                                          CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ExtractJobPostingFromUrlCommand(request.Url), cancellationToken);

        return result.Match<IActionResult>(Ok, err => err is Unauthorized401Exception ? Unauthorized(err.MapToResponse()) : BadRequest(err.MapToResponse()));
    }
}
