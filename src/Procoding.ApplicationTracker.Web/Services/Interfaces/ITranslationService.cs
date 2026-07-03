using FluentResults;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Request.Translations;
using Procoding.ApplicationTracker.DTOs.Response.Translations;

namespace Procoding.ApplicationTracker.Web.Services.Interfaces;

public interface ITranslationService
{
    /// <summary>Gets all translations (all keys, all languages).</summary>
    Task<Result<TranslationsResponseDTO>> GetTranslationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates or updates a translation value (admin only).</summary>
    Task<Result<TranslationDTO>> UpsertTranslationAsync(UpsertTranslationRequestDTO request, CancellationToken cancellationToken = default);
}
