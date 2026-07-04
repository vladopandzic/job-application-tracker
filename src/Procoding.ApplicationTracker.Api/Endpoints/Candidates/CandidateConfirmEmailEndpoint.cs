using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.Application.Candidates.Commands.ConfirmCandidateEmail;
using Procoding.ApplicationTracker.DTOs.Request.Candidates;

namespace Procoding.ApplicationTracker.Api.Endpoints.Candidates;

/// <summary>
/// Landing endpoint for the email-confirmation link. Confirms the account, then redirects the browser
/// to the web login page with a status flag so the user gets a friendly message.
/// </summary>
public class CandidateConfirmEmailEndpoint : EndpointBaseAsync.WithRequest<CandidateConfirmEmailRequestDTO>.WithResult<IActionResult>
{
    private readonly ISender _sender;
    private readonly IConfiguration _configuration;

    public CandidateConfirmEmailEndpoint(ISender sender, IConfiguration configuration)
    {
        _sender = sender;
        _configuration = configuration;
    }

    [HttpGet("candidates/confirm-email")]
    public override async Task<IActionResult> HandleAsync([FromQuery] CandidateConfirmEmailRequestDTO request,
                                                          CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ConfirmCandidateEmailCommand(request.UserId, request.Token), cancellationToken);

        var webLoginUrl = _configuration["EmailConfirmation:WebLoginUrl"] ?? "https://jobtrek.runasp.net/Login";

        return result.Match<IActionResult>(
            _ => Redirect($"{webLoginUrl}?confirmed=1"),
            _ => Redirect($"{webLoginUrl}?confirmError=1"));
    }
}
