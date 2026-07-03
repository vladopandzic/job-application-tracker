using Procoding.ApplicationTracker.DTOs.Model;

namespace Procoding.ApplicationTracker.DTOs.Response.Translations;

/// <summary>
/// All translations (every key, every language).
/// </summary>
public sealed class TranslationsResponseDTO
{
    public TranslationsResponseDTO(IReadOnlyCollection<TranslationDTO> translations)
    {
        Translations = translations;
    }

    public TranslationsResponseDTO()
    {
        Translations = new List<TranslationDTO>();
    }

    public IReadOnlyCollection<TranslationDTO> Translations { get; set; }
}
