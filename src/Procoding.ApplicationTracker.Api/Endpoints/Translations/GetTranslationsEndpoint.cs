using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.Application.Translations.Queries.GetTranslations;
using Procoding.ApplicationTracker.DTOs.Response.Translations;

namespace Procoding.ApplicationTracker.Api.Endpoints.Translations;

public class GetTranslationsEndpoint : EndpointBaseAsync.WithoutRequest.WithResult<TranslationsResponseDTO>
{
    private readonly ISender _sender;

    public GetTranslationsEndpoint(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("translations")]
    [AllowAnonymous]
    public override Task<TranslationsResponseDTO> HandleAsync(CancellationToken cancellationToken = default)
    {
        return _sender.Send(new GetTranslationsQuery(), cancellationToken);
    }
}
