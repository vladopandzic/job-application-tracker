namespace Procoding.ApplicationTracker.Infrastructure.AiExtraction;

/// <summary>
/// Google Gemini settings, bound from the "Gemini" configuration section. ApiKey comes from a secret at
/// deploy time — never hardcoded. When ApiKey is empty the extractor no-ops (feature simply unavailable).
/// </summary>
public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Free-tier Flash model by default; overridable without a code change.</summary>
    public string Model { get; set; } = "gemini-2.0-flash";
}
