using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.Api.Extensions;
using Procoding.ApplicationTracker.Application.Translations.Commands.UpsertTranslation;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Request.Translations;

namespace Procoding.ApplicationTracker.Api.Endpoints.Translations;

public class UpsertTranslationEndpoint : EndpointBaseAsync.WithRequest<UpsertTranslationRequestDTO>.WithResult<IActionResult>
{
    private readonly ISender _sender;

    public UpsertTranslationEndpoint(ISender sender)
    {
        _sender = sender;
    }

    [HttpPut("translations")]
    [ProducesResponseType(typeof(TranslationDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthenticationSchemes = "BearerEmployee,BearerCandidate", Policy = Policies.EmployeeOnly)]
    public override async Task<IActionResult> HandleAsync(UpsertTranslationRequestDTO request, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new UpsertTranslationCommand(request.Key, request.LanguageCode, request.Value), cancellationToken);

        return result.Match<IActionResult>(dto => Ok(dto), err => BadRequest(err.MapToResponse()));
    }
}
