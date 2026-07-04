using Procoding.ApplicationTracker.Web.Services.Interfaces;

namespace Procoding.ApplicationTracker.Web.Localization;

/// <summary>
/// Server-side (MVC / Razor views) counterpart of <see cref="LocalizationService"/>. The auth pages
/// (login / signup / admin) are MVC .cshtml, not Blazor, so they can't use the scoped circuit service.
/// This scoped helper pulls the SAME DB-backed translations (via the anonymous <c>GET /translations</c>
/// endpoint) so auth text is admin-editable too — no hardcoded strings in the views.
/// Scoped: loads once per request, so admin edits show up immediately.
/// </summary>
public sealed class ServerSideTranslations
{
    public const string DefaultLanguage = "hr";

    private readonly ITranslationService _translationService;
    private Dictionary<string, Dictionary<string, string>> _byKey = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    public ServerSideTranslations(ITranslationService translationService)
    {
        _translationService = translationService;
    }

    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (_loaded)
        {
            return;
        }

        var result = await _translationService.GetTranslationsAsync(cancellationToken);
        if (result.IsSuccess)
        {
            _byKey = result.Value.Translations
                .GroupBy(t => t.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(x => x.LanguageCode, x => x.Value, StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);
        }

        _loaded = true;
    }

    /// <summary>Resolves a key for the given language, falling back to hr, then the key itself.</summary>
    public string T(string key, string? languageCode)
    {
        var lang = string.IsNullOrWhiteSpace(languageCode) ? DefaultLanguage : languageCode;

        if (_byKey.TryGetValue(key, out var langs))
        {
            if (langs.TryGetValue(lang, out var value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (langs.TryGetValue(DefaultLanguage, out var fallback) && !string.IsNullOrEmpty(fallback))
            {
                return fallback;
            }
        }

        return key;
    }
}
