using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Procoding.ApplicationTracker.Application.Core.Abstractions.AiExtraction;

namespace Procoding.ApplicationTracker.Infrastructure.AiExtraction;

/// <summary>
/// Extracts job posting fields via Google Gemini's generateContent REST API, forcing JSON output.
/// Best-effort: returns null on any failure (unconfigured key, HTTP error, bad JSON) so the caller can
/// degrade gracefully — the feature never throws into the request pipeline.
/// </summary>
internal sealed class GeminiJobPostingExtractor : IJobPostingExtractor
{
    private const int MaxContentChars = 12000; // guard token cost — postings rarely need more

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiJobPostingExtractor> _logger;

    public GeminiJobPostingExtractor(HttpClient httpClient, IOptions<GeminiOptions> options, ILogger<GeminiJobPostingExtractor> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ExtractedJobPosting?> ExtractAsync(string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(content))
        {
            _logger.LogInformation("Gemini not configured or empty content — skipping extraction.");
            return null;
        }

        var trimmed = content.Length > MaxContentChars ? content[..MaxContentChars] : content;

        var instruction =
            "You extract structured data from a job posting. Respond with ONLY a JSON object with exactly these keys: " +
            "\"companyName\" (string), \"positionTitle\" (string), " +
            "\"jobType\" (one of \"FullTime\",\"PartTime\",\"Contract\",\"Temporary\",\"Volunteer\", or null), " +
            "\"workLocationType\" (one of \"Remote\",\"OnSite\",\"Hybrid\", or null), " +
            "\"description\" (a concise summary in the posting's original language), " +
            "\"companyWebsite\" (the company's website URL or null), " +
            "\"jobAdLink\" (the direct URL to this job posting / application page if present in the text, else null). " +
            "Use null when a field is not stated. Do not invent values.";

        var requestBody = new
        {
            systemInstruction = new { parts = new[] { new { text = instruction } } },
            contents = new[] { new { role = "user", parts = new[] { new { text = trimmed } } } },
            generationConfig = new { temperature = 0, responseMimeType = "application/json" }
        };

        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";

            using var response = await _httpClient.PostAsJsonAsync(url, requestBody, JsonOpts, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini extraction failed: {Status} {Body}", response.StatusCode, body);
                return null;
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

            var text = doc.RootElement
                          .GetProperty("candidates")[0]
                          .GetProperty("content")
                          .GetProperty("parts")[0]
                          .GetProperty("text")
                          .GetString();

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var parsed = JsonSerializer.Deserialize<GeminiExtractionDto>(text, JsonOpts);

            if (parsed is null || (string.IsNullOrWhiteSpace(parsed.CompanyName) && string.IsNullOrWhiteSpace(parsed.PositionTitle)))
            {
                return null;
            }

            return new ExtractedJobPosting(
                CompanyName: parsed.CompanyName ?? string.Empty,
                PositionTitle: parsed.PositionTitle ?? string.Empty,
                JobType: Normalize(parsed.JobType),
                WorkLocationType: Normalize(parsed.WorkLocationType),
                Description: parsed.Description ?? string.Empty,
                CompanyWebsite: Normalize(parsed.CompanyWebsite),
                JobAdLink: Normalize(parsed.JobAdLink));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini extraction threw for content of length {Length}.", trimmed.Length);
            return null;
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) || string.Equals(value, "null", StringComparison.OrdinalIgnoreCase) ? null : value;

    private sealed class GeminiExtractionDto
    {
        public string? CompanyName { get; set; }
        public string? PositionTitle { get; set; }
        public string? JobType { get; set; }
        public string? WorkLocationType { get; set; }
        public string? Description { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? JobAdLink { get; set; }
    }
}
