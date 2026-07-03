namespace Procoding.ApplicationTracker.DTOs.Model;

/// <summary>
/// One translated UI string (a key + language + value).
/// </summary>
public class TranslationDTO
{
    public TranslationDTO()
    {
    }

    public TranslationDTO(string key, string languageCode, string value)
    {
        Key = key;
        LanguageCode = languageCode;
        Value = value;
    }

    public string Key { get; set; } = "";

    public string LanguageCode { get; set; } = "";

    public string Value { get; set; } = "";
}
