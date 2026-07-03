using FluentResults;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Request.Translations;
using Procoding.ApplicationTracker.DTOs.Response.Translations;
using Procoding.ApplicationTracker.Web.Auth;
using Procoding.ApplicationTracker.Web.Extensions;
using Procoding.ApplicationTracker.Web.Services.Interfaces;
using System.Net.Http.Headers;

namespace Procoding.ApplicationTracker.Web.Services;

public class TranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenProvider _tokenProvider;

    public TranslationService(HttpClient httpClient, ITokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
    }

    public async Task<Result<TranslationsResponseDTO>> GetTranslationsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(UrlConstants.Translations.GET_ALL_URL, cancellationToken);

        return await response.HandleResponseAsync<TranslationsResponseDTO>(cancellationToken);
    }

    public async Task<Result<TranslationDTO>> UpsertTranslationAsync(UpsertTranslationRequestDTO request, CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenProvider.GetAccessTokenAsync());

        var response = await _httpClient.PutAsJsonAsync(UrlConstants.Translations.GET_ALL_URL, request, cancellationToken);

        return await response.HandleResponseAsync<TranslationDTO>(cancellationToken);
    }
}
