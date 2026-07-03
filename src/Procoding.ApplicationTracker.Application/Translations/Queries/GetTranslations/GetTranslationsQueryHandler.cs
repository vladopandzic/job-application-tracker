using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Response.Translations;

namespace Procoding.ApplicationTracker.Application.Translations.Queries.GetTranslations;

internal sealed class GetTranslationsQueryHandler : IQueryHandler<GetTranslationsQuery, TranslationsResponseDTO>
{
    private readonly ITranslationRepository _translationRepository;

    public GetTranslationsQueryHandler(ITranslationRepository translationRepository)
    {
        _translationRepository = translationRepository;
    }

    public async Task<TranslationsResponseDTO> Handle(GetTranslationsQuery request, CancellationToken cancellationToken)
    {
        var translations = await _translationRepository.GetAllAsync(cancellationToken);

        var dtos = translations
            .Select(t => new TranslationDTO(t.Key, t.LanguageCode, t.Value))
            .ToList();

        return new TranslationsResponseDTO(dtos);
    }
}
