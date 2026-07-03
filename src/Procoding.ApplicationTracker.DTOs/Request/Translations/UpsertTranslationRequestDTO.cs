namespace Procoding.ApplicationTracker.DTOs.Request.Translations;

/// <summary>
/// Request to create or update a translation value for a key + language.
/// </summary>
public record UpsertTranslationRequestDTO(string Key, string LanguageCode, string Value);
