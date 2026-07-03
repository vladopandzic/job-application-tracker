using Blazored.LocalStorage;
using Microsoft.JSInterop;
using Procoding.ApplicationTracker.Web.Services.Interfaces;

namespace Procoding.ApplicationTracker.Web.Localization;

/// <summary>
/// Holds the current language and the translations loaded from the API, and resolves keys to text.
/// Scoped per user/circuit. Default language is Croatian ("hr").
/// </summary>
public class LocalizationService
{
    public const string DefaultLanguage = "hr";
    private const string StorageKey = "jf-lang";

    private readonly ITranslationService _translationService;
    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _js;

    private Dictionary<string, Dictionary<string, string>> _byKey = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    public LocalizationService(ITranslationService translationService, ILocalStorageService localStorage, IJSRuntime js)
    {
        _translationService = translationService;
        _localStorage = localStorage;
        _js = js;
    }

    /// <summary>
    /// Mirrors the current language into a cookie so the server-rendered MVC auth pages
    /// (login / signup / admin login) can pick it up — they can't read localStorage.
    /// </summary>
    private async Task SyncLanguageCookieAsync(string languageCode)
    {
        try
        {
            await _js.InvokeVoidAsync("eval", $"document.cookie='jf-lang={languageCode};path=/;max-age=31536000;SameSite=Lax'");
        }
        catch
        {
            // JS not available (prerender) — ignore.
        }
    }

    /// <summary>Raised when the language changes so subscribed components can re-render.</summary>
    public event Action? OnChange;

    public string CurrentLanguage { get; private set; } = DefaultLanguage;

    public IReadOnlyList<(string Code, string Label)> Languages { get; } = new[]
    {
        ("hr", "Hrvatski"),
        ("en", "English"),
    };

    /// <summary>Loads translations from the API once (subsequent calls are no-ops).</summary>
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
            _loaded = true;
        }
    }

    /// <summary>Reads the saved language from local storage (client-side only). Call after first render.</summary>
    public async Task LoadLanguageFromStorageAsync()
    {
        try
        {
            var saved = await _localStorage.GetItemAsync<string>(StorageKey);
            if (!string.IsNullOrWhiteSpace(saved) && !string.Equals(saved, CurrentLanguage, StringComparison.OrdinalIgnoreCase))
            {
                CurrentLanguage = saved;
                OnChange?.Invoke();
            }

            // Keep the cookie in sync with the resolved language so MVC auth pages match.
            await SyncLanguageCookieAsync(CurrentLanguage);
        }
        catch
        {
            // local storage not available (e.g. during prerender) — keep the default.
        }
    }

    /// <summary>Resolves a key to text for the current language, falling back to the default language, then the key.</summary>
    public string T(string key)
    {
        if (_byKey.TryGetValue(key, out var langs))
        {
            if (langs.TryGetValue(CurrentLanguage, out var value) && !string.IsNullOrEmpty(value))
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

    /// <summary>
    /// Resolves a data-derived value (job type, work location, step type, outcome, status) to localized
    /// text using the key <c>{prefix}.{value}</c>. Falls back to the raw value if no translation exists,
    /// so unknown/custom values still render sensibly instead of showing the key.
    /// </summary>
    public string TValue(string prefix, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value ?? string.Empty;
        }

        var key = $"{prefix}.{value}";
        var resolved = T(key);
        return resolved == key ? value : resolved;
    }

    public async Task SetLanguageAsync(string languageCode)
    {
        if (string.Equals(languageCode, CurrentLanguage, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        CurrentLanguage = languageCode;

        try
        {
            await _localStorage.SetItemAsync(StorageKey, languageCode);
        }
        catch
        {
            // ignore
        }

        await SyncLanguageCookieAsync(languageCode);

        OnChange?.Invoke();
    }

    /// <summary>Updates a cached translation value (e.g. after an admin edit) without reloading everything.</summary>
    public void ReplaceTranslation(string key, string languageCode, string value)
    {
        if (!_byKey.TryGetValue(key, out var langs))
        {
            langs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _byKey[key] = langs;
        }

        langs[languageCode] = value;
        OnChange?.Invoke();
    }
}
